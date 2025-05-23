using Application.Interfaces;
using Domain.DTO.Request;
using Domain.DTO.Response;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Infrastructure.Clients
{
    public class InventoryServiceClient : IInventoryServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<InventoryServiceClient> _logger;
        public InventoryServiceClient(HttpClient httpClient, ILogger<InventoryServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }


        public async Task<List<ProductResponse>?> GetAllProductsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("products/view-all");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<ProductResponse>>();
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi gọi InventoryService: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> UpdateStockAfterOrderAsync(int warehouseId, List<OrderDetail> orderDetails)
        {
            try
            {
                // Chuyển đổi danh sách OrderDetail thành StockUpdateRequest
                var stockUpdateRequest = new StockUpdateRequest
                {
                    WarehouseId = warehouseId,
                    Items = orderDetails.Select(od => new StockItemResponseOrder
                    {
                        VariantId = od.ProductVariantId,
                        Quantity = od.Quantity
                    }).ToList()
                };

                Console.WriteLine($"[DEBUG] Payload gửi đi: {JsonSerializer.Serialize(stockUpdateRequest)}");

                // Gửi request đến InventoryService
                var response = await _httpClient.PostAsJsonAsync("warehouses/update-after-order", stockUpdateRequest);
                var responseJson = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[DEBUG] API Response: {responseJson}");

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[ERROR] Không thể cập nhật tồn kho: {response.StatusCode}");
                    return false;
                }

                // 🛠 Đọc JSON đúng kiểu `ResponseDTO`
                var result = await response.Content.ReadFromJsonAsync<ResponseDTO>();

                if (result != null && result.Status)
                {
                    Console.WriteLine($"[INFO] {result.Message}");
                    return true;
                }
                else
                {
                    Console.WriteLine($"[ERROR] Cập nhật tồn kho thất bại: {result?.Message}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Lỗi khi gọi UpdateStockAfterOrderAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> RestoreStockAfterCancelAsync(int warehouseId, List<OrderDetail> orderDetails)
        {
            try
            {
                // Tạo request khôi phục tồn kho
                var stockUpdateRequest = new StockUpdateRequest
                {
                    WarehouseId = warehouseId,
                    Items = orderDetails.Select(od => new StockItemResponseOrder
                    {
                        VariantId = od.ProductVariantId,
                        Quantity = od.Quantity
                    }).ToList()
                };

                Console.WriteLine($"[DEBUG] Payload gửi đi (restore): {JsonSerializer.Serialize(stockUpdateRequest)}");

                // Gửi đến API InventoryService
                var response = await _httpClient.PostAsJsonAsync("warehouses/restore-after-cancel", stockUpdateRequest);
                var responseJson = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[DEBUG] API Response (restore): {responseJson}");

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[ERROR] Không thể khôi phục tồn kho: {response.StatusCode}");
                    return false;
                }

                var result = await response.Content.ReadFromJsonAsync<ResponseDTO>();

                if (result != null && result.Status)
                {
                    Console.WriteLine($"[INFO] {result.Message}");
                    return true;
                }
                else
                {
                    Console.WriteLine($"[ERROR] Khôi phục tồn kho thất bại: {result?.Message}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Lỗi khi gọi RestoreStockAfterCancelAsync: {ex.Message}");
                return false;
            }
        }


        public async Task<int> GetStockQuantityAsync(int storeId, int variantId)
        {
            try
            {
                // Gọi endpoint GET api/inventory/stock?storeId={storeId}&variantId={variantId}
                var response = await _httpClient.GetAsync($"stores/{storeId}/stock/{variantId}");
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[ERROR] Không thể lấy tồn kho: {response.StatusCode}");
                    return 0;
                }

                var responseData = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[DEBUG] Stock API Response: {responseData}");

                // Giả sử API trả về ResponseDTO<int> chứa số lượng tồn kho
                var result = JsonSerializer.Deserialize<ResponseDTO<StockQuantityResponse>>(responseData, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });


                return result?.Data.StockQuantity ?? 0;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"[ERROR] Lỗi kết nối đến InventoryService: {ex.Message}");
                return 0;
            }
        }
        public async Task<ProductDetailResponse?> GetProductByIdAsync(int productId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"products/{productId}");

                if (response.IsSuccessStatusCode)
                {
                    var apiResult = await response.Content.ReadFromJsonAsync<ResponseDTO<ProductDetailResponse>>();

                    if (apiResult != null && apiResult.Status)
                    {
                        return apiResult.Data;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi gọi InventoryService: {ex.Message}");
                return null;
            }
        }

        public async Task<List<ProductResponse>?> GetProductsByStyleNameAsync(string styleName, int page, int pageSize)
        {
            try
            {
                var response = await _httpClient.GetAsync($"products/by-style?styleName={Uri.EscapeDataString(styleName)}&page={page}&pageSize={pageSize}");

                if (response.IsSuccessStatusCode)
                {
                    var apiResult = await response.Content.ReadFromJsonAsync<ResponseDTO<List<ProductResponse>>>();

                    if (apiResult != null && apiResult.Status)
                    {
                        return apiResult.Data;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi gọi InventoryService (GetProductsByStyleNameAsync): {ex.Message}");
                return null;
            }
        }
        public async Task<ProductVariantResponse?> GetProductVariantByIdAsync(int productVariantId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"products/variant/{productVariantId}");
                var responseData = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[DEBUG] API Response: {responseData}");

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[ERROR] Không thể lấy thông tin sản phẩm: {response.StatusCode}");
                    return null;
                }

                // Deserialize thành ResponseDTO<ProductVariant> và trả về Data
                var result = JsonSerializer.Deserialize<ResponseDTO<ProductVariantResponse>>(responseData, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });


                return result?.Data;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"[ERROR] Lỗi kết nối đến InventoryService: {ex.Message}");
                return null;
            }
        }
        public async Task<Dictionary<int, ProductVariantResponse>> GetAllProductVariantsByIdsAsync(List<int> variantIds)
        {
            if (variantIds == null || !variantIds.Any())
            {
                return new Dictionary<int, ProductVariantResponse>();
            }

            try
            {
                var response = await _httpClient.PostAsJsonAsync("products/variants/details", variantIds);
                response.EnsureSuccessStatusCode();

                // 🛠 Đọc dữ liệu đúng kiểu ResponseDTO<List<ProductVariantResponse>>
                var responseDTO = await response.Content.ReadFromJsonAsync<ResponseDTO<List<ProductVariantResponse>>>();

                // ✅ Lấy danh sách từ responseDTO.Data
                return responseDTO?.Data?.ToDictionary(v => v.VariantId) ?? new Dictionary<int, ProductVariantResponse>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"Error fetching product variants: {ex.Message}");
                return new Dictionary<int, ProductVariantResponse>();
            }
        }

     
        public async Task<ProductVariantResponse?> GetProductVariantById(int variantId)
        {
            var response = await _httpClient.GetAsync($"products/variant/{variantId}");

            if (!response.IsSuccessStatusCode)
                return null;

            var result = await response.Content.ReadFromJsonAsync<ResponseDTO<ProductVariantResponse>>();
            return result?.Data;
        }
        public async Task<ProductVariantResponse?> GetProductVariantByDetails(int productId, string size, string color)
        {
            try
            {
                var response = await _httpClient.GetAsync($"products/variant/details?productId={productId}&size={Uri.EscapeDataString(size)}&color={Uri.EscapeDataString(color)}");

                // ❌ Kiểm tra nếu response không thành công
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"InventoryService trả về mã lỗi {response.StatusCode} khi tìm biến thể sản phẩm.");
                    return null;
                }

                // ✅ Đọc JSON từ response
                var result = await response.Content.ReadFromJsonAsync<ResponseDTO<ProductVariantResponse>>();

                // ❌ Kiểm tra dữ liệu null hoặc API báo lỗi
                if (result == null || !result.Status)
                {
                    _logger.LogWarning($"InventoryService phản hồi lỗi: {result?.Message ?? "Không có dữ liệu"}");
                    return null;
                }

                return result.Data;
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError($"Lỗi HTTP khi gọi InventoryService: {httpEx.Message}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi không xác định khi gọi InventoryService: {ex.Message}");
                return null;
            }
        }


    }
}
