using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface ICartRepository
    {
        // Lấy giỏ hàng từ Database cho account
        Task<List<CartItem>> GetCartFromDatabaseAsync(int accountId);

        // Thêm sản phẩm vào giỏ hàng (DB)
        Task AddToCartAsync(int accountId, CartItem cartItem);

        // Xóa một sản phẩm khỏi giỏ hàng (DB)
        Task RemoveFromCartAsync(int accountId, int productVariantId);

        // Đồng bộ giỏ hàng (ví dụ: từ cache) sang Database
        Task SyncCartToDatabase(int accountId, List<CartItem> cartItems);

        // Xóa toàn bộ giỏ hàng trong Database
        Task ClearCartInDatabase(int accountId);
        Task RemoveSelectedItemsFromCart(int accountId, List<int> selectedProductVariantIds);
        Task UpdateCartItemQuantityAsync(int accountId, int productVariantId, int newQuantity);

    }

}
