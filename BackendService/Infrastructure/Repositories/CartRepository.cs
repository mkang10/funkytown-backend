using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly FtownContext _context;

        public CartRepository(FtownContext context)
        {
            _context = context;
        }

        // Lấy giỏ hàng từ DB dựa trên AccountId
        public async Task<List<CartItem>> GetCartFromDatabaseAsync(int accountId)
        {
            return await _context.CartItems
                .Where(c => c.Cart.AccountId == accountId)
                .ToListAsync();
        }

        // Thêm sản phẩm vào giỏ hàng (DB)
        public async Task AddToCartAsync(int accountId, CartItem cartItem)
        {
            // Tìm ShoppingCart của account (nếu chưa có, tạo mới)
            var shoppingCart = await _context.ShoppingCarts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.AccountId == accountId);

            if (shoppingCart == null)
            {
                shoppingCart = new ShoppingCart
                {
                    AccountId = accountId,
                    CreatedDate = DateTime.UtcNow,
                    CartItems = new List<CartItem>()
                };
                _context.ShoppingCarts.Add(shoppingCart);
            }

            // Kiểm tra sản phẩm đã có trong giỏ hàng chưa
            var existingItem = shoppingCart.CartItems
                .FirstOrDefault(c => c.ProductVariantId == cartItem.ProductVariantId);

            if (existingItem != null)
            {
                existingItem.Quantity += cartItem.Quantity;
                _context.CartItems.Update(existingItem);
            }
            else
            {
                // Gán CartId hợp lệ cho cartItem
                cartItem.CartId = shoppingCart.CartId;
                shoppingCart.CartItems.Add(cartItem);
            }

            await _context.SaveChangesAsync();
        }

        // Xóa hoàn toàn một sản phẩm khỏi giỏ hàng (DB)
        public async Task RemoveFromCartAsync(int accountId, int productVariantId)
        {
            var shoppingCart = await _context.ShoppingCarts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.AccountId == accountId);

            if (shoppingCart == null) return;

            var cartItem = shoppingCart.CartItems
                .FirstOrDefault(c => c.ProductVariantId == productVariantId);

            if (cartItem == null) return;

            _context.CartItems.Remove(cartItem);

            await _context.SaveChangesAsync();
        }

        // Đồng bộ giỏ hàng từ cache sang DB
        public async Task SyncCartToDatabase(int accountId, List<CartItem> cartItems)
        {
            // Tìm hoặc tạo ShoppingCart cho account
            var shoppingCart = await _context.ShoppingCarts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.AccountId == accountId);

            if (shoppingCart == null)
            {
                shoppingCart = new ShoppingCart
                {
                    AccountId = accountId,
                    CreatedDate = DateTime.UtcNow,
                    CartItems = new List<CartItem>()
                };
                _context.ShoppingCarts.Add(shoppingCart);
                await _context.SaveChangesAsync(); // Lưu để có CartId hợp lệ
            }

            // Xóa các mục cũ trong giỏ hàng DB để tránh trùng lặp
            _context.CartItems.RemoveRange(shoppingCart.CartItems);
            await _context.SaveChangesAsync();

            // Thêm giỏ hàng mới từ cache
            foreach (var item in cartItems)
            {
                item.CartId = shoppingCart.CartId;
                _context.CartItems.Add(item);
            }
            await _context.SaveChangesAsync();
        }

        // Xóa toàn bộ giỏ hàng trong DB
        public async Task ClearCartInDatabase(int accountId)
        {
            var shoppingCart = await _context.ShoppingCarts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.AccountId == accountId);

            if (shoppingCart != null)
            {
                _context.CartItems.RemoveRange(shoppingCart.CartItems);
                _context.ShoppingCarts.Remove(shoppingCart);
                await _context.SaveChangesAsync();
            }
        }
        public async Task RemoveSelectedItemsFromCart(int accountId, List<int> selectedProductVariantIds)
        {
            var cart = await _context.ShoppingCarts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.AccountId == accountId);

            if (cart == null || cart.CartItems == null)
                return;

            // ✅ Xóa các sản phẩm được chọn khỏi giỏ hàng
            var itemsToRemove = cart.CartItems
                .Where(item => selectedProductVariantIds.Contains(item.ProductVariantId))
                .ToList();

            _context.CartItems.RemoveRange(itemsToRemove); // ✅ Xóa chỉ CartItem
            await _context.SaveChangesAsync(); // ✅ Lưu thay đổi

            // ❌ Không xóa ShoppingCart (Không cần kiểm tra giỏ hàng trống)
        }

        public async Task UpdateCartItemQuantityAsync(int accountId, int productVariantId, int newQuantity)
        {
            var cart = await _context.ShoppingCarts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.AccountId == accountId);

            if (cart == null) return;

            var item = cart.CartItems.FirstOrDefault(ci => ci.ProductVariantId == productVariantId);
            if (item != null)
            {
                item.Quantity = newQuantity;
                await _context.SaveChangesAsync();
            }
        }

    }
}
