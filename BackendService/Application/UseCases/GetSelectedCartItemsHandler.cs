using Application.Interfaces;
using Domain.DTO.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class GetSelectedCartItemsHandler
    {
        private readonly ICustomerServiceClient _customerServiceClient;
        private readonly IInventoryServiceClient _inventoryServiceClient;

        public GetSelectedCartItemsHandler(ICustomerServiceClient customerServiceClient, IInventoryServiceClient inventoryServiceClient)
        {
            _customerServiceClient = customerServiceClient;
            _inventoryServiceClient = inventoryServiceClient;
        }

        public async Task<List<OrderItemRequest>?> Handle(int accountId, List<int> selectedProductVariantIds)
        {
            // ✅ 1. Lấy toàn bộ giỏ hàng từ Redis thông qua CustomerServiceClient
            var cartItems = await _customerServiceClient.GetCartAsync(accountId);
            if (cartItems == null || !cartItems.Any())
                return null;

            var orderItems = new List<OrderItemRequest>();

            // ✅ 2. Lọc các sản phẩm được chọn từ giỏ hàng
            foreach (var productVariantId in selectedProductVariantIds)
            {
                var cartItem = cartItems.FirstOrDefault(c => c.ProductVariantId == productVariantId);
                if (cartItem == null)
                    return null;

                // ✅ 3. Lấy thông tin `variant` từ InventoryServiceClient (là ProductVariantResponse)
                var productVariantResponse = await _inventoryServiceClient.GetProductVariantByIdAsync(cartItem.ProductVariantId);
                if (productVariantResponse == null)
                    return null;

                // ✅ 4. Thêm vào danh sách order item
                orderItems.Add(new OrderItemRequest
                {
                    ProductVariantId = productVariantResponse.VariantId,
                    ProductName = productVariantResponse.ProductName,
                    ImageUrl = productVariantResponse.ImagePath,
                    Size = productVariantResponse.Size,
                    Color = productVariantResponse.Color,
                    Quantity = cartItem.Quantity, // ✅ Tự động lấy số lượng từ giỏ hàng
                    Price = productVariantResponse.Price, // ✅ Giá gốc từ ProductVariantResponse
                    DiscountedPrice = productVariantResponse.DiscountedPrice, // ✅ Giá sau giảm giá từ ProductVariantResponse
                    PromotionTitle = productVariantResponse.PromotionTitle // ✅ Tên khuyến mãi từ ProductVariantResponse
                });
            }

            return orderItems;
        }

    }
}
