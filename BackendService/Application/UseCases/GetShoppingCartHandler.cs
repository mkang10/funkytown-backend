using Application.Interfaces;
using AutoMapper;
using Domain.DTO.Request;
using Domain.DTO.Response;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using IDatabase = StackExchange.Redis.IDatabase;

namespace Application.UseCases
{
    public class GetShoppingCartHandler
    {
        private readonly ICartRepository _cartRepository;
        private readonly IInventoryServiceClient _inventoryServiceClient;
        private readonly IMapper _mapper;
        private readonly IRedisCacheService _redisCacheService;

        public GetShoppingCartHandler(
            ICartRepository cartRepository,
            IInventoryServiceClient inventoryServiceClient,
            IMapper mapper,
            IRedisCacheService redisCacheService)
        {
            _cartRepository = cartRepository;
            _inventoryServiceClient = inventoryServiceClient;
            _mapper = mapper;
            _redisCacheService = redisCacheService;
        }

        private string GetCartKey(int accountId) => $"cart:{accountId}";

        public async Task<ResponseDTO<List<CartItemResponse>>> Handle(int accountId)
        {
            var cartKey = GetCartKey(accountId);

            // Lấy giỏ hàng từ Redis hoặc DB
            var cart = await _redisCacheService.GetCacheAsync<List<CartItem>>(cartKey) ?? new List<CartItem>();
            if (!cart.Any())
            {
                Console.WriteLine("⏳ Cache miss: Lấy giỏ hàng từ Database...");
                cart = await _cartRepository.GetCartFromDatabaseAsync(accountId);
                if (cart.Any())
                {
                    await _redisCacheService.SetCacheAsync(cartKey, cart, TimeSpan.FromMinutes(1));
                }
            }

            if (!cart.Any())
            {
                return new ResponseDTO<List<CartItemResponse>>(new List<CartItemResponse>(), true, "Giỏ hàng trống.");
            }

            var cartItemResponses = cart.Select(cartItem => new CartItemResponse
            {
                ProductVariantId = cartItem.ProductVariantId,
                Quantity = cartItem.Quantity
            }).ToList();

            // Kiểm tra tồn kho
            var tasks = cartItemResponses.Select(async item =>
            {
                var productVariant = await _inventoryServiceClient.GetProductVariantById(item.ProductVariantId);
                if (productVariant != null)
                {
                    item.ProductName = productVariant.ProductName;
                    item.ImagePath = productVariant.ImagePath;
                    item.Size = productVariant.Size;
                    item.Color = productVariant.Color;
                    item.Price = productVariant.Price;
                    item.DiscountedPrice = productVariant.DiscountedPrice;
                    item.PromotionTitle = productVariant.PromotionTitle;

                    // ❗ Kiểm tra tồn kho
                    if (item.Quantity > productVariant.StockQuantity)
                    {
                        item.Message = $"Sản phẩm chỉ còn {productVariant.StockQuantity} trong kho.";
                        // Nếu muốn cập nhật lại số lượng để phản ánh đúng:
                        // item.Quantity = productVariant.StockQuantity;
                    }
                }
                else
                {
                    item.Message = "Sản phẩm không tồn tại hoặc đã bị xoá.";
                }
            });
            await Task.WhenAll(tasks);


            return new ResponseDTO<List<CartItemResponse>>(cartItemResponses, true, "Lấy giỏ hàng thành công!");
        }


        public async Task<ResponseDTO<bool>> AddCartItem(int accountId, AddToCartRequest cartItemDto)
        {
            // Lấy ProductVariant từ InventoryService dựa trên ProductId, Size, Color
            var productVariant = await _inventoryServiceClient.GetProductVariantByDetails(cartItemDto.ProductId, cartItemDto.Size, cartItemDto.Color);

            if (productVariant == null)
            {
                return new ResponseDTO<bool>(false, false, "Sản phẩm với kích thước và màu sắc không tồn tại!");
            }

            var cartKey = GetCartKey(accountId);

            // Lấy giỏ hàng từ Redis; nếu không có thì tải từ DB
            var cart = await _redisCacheService.GetCacheAsync<List<CartItem>>(cartKey);
            if (cart == null)
            {
                cart = await _cartRepository.GetCartFromDatabaseAsync(accountId) ?? new List<CartItem>();
            }

            // Kiểm tra xem sản phẩm đã có trong giỏ hàng chưa
            var existingItem = cart.FirstOrDefault(c => c.ProductVariantId == productVariant.VariantId);
            int totalQuantityAfterAdding = cartItemDto.Quantity + (existingItem?.Quantity ?? 0);

            // So sánh với tồn kho
            if (totalQuantityAfterAdding > productVariant.StockQuantity)
            {
                return new ResponseDTO<bool>(false, false, "Số lượng sản phẩm trong giỏ hàng vượt quá tồn kho!");
            }

            // Cập nhật giỏ hàng trong bộ nhớ
            if (existingItem != null)
            {
                existingItem.Quantity += cartItemDto.Quantity;
            }
            else
            {
                cart.Add(new CartItem
                {
                    ProductVariantId = productVariant.VariantId,
                    Quantity = cartItemDto.Quantity
                });
            }

            // Cập nhật dữ liệu vào DB qua repository
            await _cartRepository.AddToCartAsync(accountId, new CartItem
            {
                ProductVariantId = productVariant.VariantId,
                Quantity = cartItemDto.Quantity
            });

            // Cập nhật cache Redis với giỏ hàng mới nhất
            await _redisCacheService.SetCacheAsync(cartKey, cart, TimeSpan.FromMinutes(30));

            return new ResponseDTO<bool>(true, true, "Thêm sản phẩm vào giỏ hàng thành công!");
        }


        // --- Remove Cart Item ---
        public async Task<ResponseDTO<bool>> RemoveCartItem(int accountId, int productVariantId)
        {
            try
            {
                await _cartRepository.RemoveFromCartAsync(accountId, productVariantId);
                await _redisCacheService.RemoveCacheAsync(GetCartKey(accountId));

                return new ResponseDTO<bool>(true, true, "Xóa sản phẩm khỏi giỏ hàng thành công.");
            }
            catch (Exception ex)
            {
                return new ResponseDTO<bool>(false, false, $"Đã xảy ra lỗi khi xóa sản phẩm: {ex.Message}");
            }
        }


        // --- Clear Cart ---
        public async Task<ResponseDTO<bool>> ClearCart(int accountId)
        {
            try
            {
                await _cartRepository.ClearCartInDatabase(accountId);
                await _redisCacheService.RemoveCacheAsync(GetCartKey(accountId));

                return new ResponseDTO<bool>(true, true, "Đã xóa toàn bộ giỏ hàng.");
            }
            catch (Exception ex)
            {
                return new ResponseDTO<bool>(false, false, $"Đã xảy ra lỗi khi xóa giỏ hàng: {ex.Message}");
            }
        }


        // --- Sync Cart to Database ---
        public async Task SyncCartToDatabase(int accountId)
        {
            // Lấy giỏ hàng từ Redis
            var cart = await _redisCacheService.GetCacheAsync<List<CartItem>>(GetCartKey(accountId));
            if (cart == null || !cart.Any()) return;

            await _cartRepository.SyncCartToDatabase(accountId, cart);
        }

        // --- Clear Cart After Order ---
        public async Task ClearCartAfterOrderAsync(int accountId, List<int> selectedProductVariantIds)
        {
            // ✅ Xóa sản phẩm đã đặt hàng khỏi DB
            await _cartRepository.RemoveSelectedItemsFromCart(accountId, selectedProductVariantIds);

            // ✅ Cập nhật lại cache Redis
            await _redisCacheService.RemoveCacheAsync(GetCartKey(accountId));
        }

        public async Task<ResponseDTO<bool>> ChangeCartItemQuantity(int accountId, ChangeCartItemQuantityRequest request)
        {
            var cartKey = GetCartKey(accountId);

            var cart = await _redisCacheService.GetCacheAsync<List<CartItem>>(cartKey);
            if (cart == null)
            {
                cart = await _cartRepository.GetCartFromDatabaseAsync(accountId) ?? new List<CartItem>();
            }

            var item = cart.FirstOrDefault(c => c.ProductVariantId == request.ProductVariantId);
            if (item == null)
            {
                return new ResponseDTO<bool>(false, false, "Sản phẩm không có trong giỏ hàng.");
            }

            var newQuantity = item.Quantity + request.QuantityChange;

            // 👉 Nếu số lượng mới <= 0 → xoá khỏi giỏ
            if (newQuantity <= 0)
            {
                cart.Remove(item);
                await _cartRepository.RemoveFromCartAsync(accountId, request.ProductVariantId);
                await _redisCacheService.SetCacheAsync(cartKey, cart, TimeSpan.FromMinutes(30));
                return new ResponseDTO<bool>(true, true, "Sản phẩm đã được xóa khỏi giỏ hàng.");
            }

            // Kiểm tra tồn kho
            var variant = await _inventoryServiceClient.GetProductVariantById(request.ProductVariantId);
            if (variant == null)
            {
                return new ResponseDTO<bool>(false, false, "Sản phẩm không tồn tại.");
            }

            if (newQuantity > variant.StockQuantity)
            {
                return new ResponseDTO<bool>(false, false, "Số lượng vượt quá tồn kho.");
            }

            // Cập nhật số lượng mới
            item.Quantity = newQuantity;
            await _cartRepository.UpdateCartItemQuantityAsync(accountId, request.ProductVariantId, newQuantity);
            await _redisCacheService.SetCacheAsync(cartKey, cart, TimeSpan.FromMinutes(30));

            return new ResponseDTO<bool>(true, true, "Cập nhật số lượng sản phẩm thành công.");
        }

    }
}
