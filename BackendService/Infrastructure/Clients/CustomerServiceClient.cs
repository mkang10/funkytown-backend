
using Application.Interfaces;
using Domain.DTO.Response;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Infrastructure.Clients
{
    public class CustomerServiceClient : ICustomerServiceClient
    {
        private readonly HttpClient _httpClient;

        public CustomerServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<CartItem>?> GetCartAsync(int accountId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"cart/{accountId}");
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[ERROR] Không thể lấy giỏ hàng. Mã lỗi: {response.StatusCode}");
                    return null;
                }

                // Deserialize theo kiểu ResponseDTO<List<CartItem>>
                var result = await response.Content.ReadFromJsonAsync<ResponseDTO<List<CartItem>>>(new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                Console.WriteLine($"[DEBUG] Dữ liệu từ API: {result}");
                return result?.Data ?? new List<CartItem>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Lỗi khi lấy giỏ hàng: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> ClearCartAfterOrderAsync(int accountId, List<int> selectedProductVariantIds)
        {
            // ✅ Gửi trực tiếp `selectedProductVariantIds`, không bọc trong một object ẩn danh
            var response = await _httpClient.PostAsJsonAsync(
                $"cart/{accountId}/clear-after-order",
                selectedProductVariantIds // ✅ Gửi đúng kiểu List<int>
            );

            return response.IsSuccessStatusCode;
        }

    }
}
