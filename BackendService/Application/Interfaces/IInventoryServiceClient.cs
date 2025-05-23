using Domain.DTO.Response;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IInventoryServiceClient
    {
        Task<List<ProductResponse>?> GetAllProductsAsync();
        Task<ProductDetailResponse?> GetProductByIdAsync(int productId);
        Task<ProductVariantResponse?> GetProductVariantByIdAsync(int productVariantId);
        Task<ProductVariantResponse?> GetProductVariantById(int variantId);
        Task<ProductVariantResponse?> GetProductVariantByDetails(int productId, string size, string color);
        Task<List<ProductResponse>?> GetProductsByStyleNameAsync(string styleName, int page, int pageSize);
        Task<int> GetStockQuantityAsync(int storeId, int variantId);
        Task<bool> UpdateStockAfterOrderAsync(int warehouseId, List<OrderDetail> orderDetails);
        Task<Dictionary<int, ProductVariantResponse?>> GetAllProductVariantsByIdsAsync(List<int> variantIds);
        Task<bool> RestoreStockAfterCancelAsync(int warehouseId, List<OrderDetail> orderDetails);
    }
}
