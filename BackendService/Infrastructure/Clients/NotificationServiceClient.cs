using Application.Interfaces;
using Domain.DTO.Request;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Clients
{
    public class NotificationServiceClient : INotificationClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<NotificationServiceClient> _logger;

        public NotificationServiceClient(HttpClient httpClient, ILogger<NotificationServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<bool> SendNotificationAsync(SendNotificationRequest request)
        {
            try
            {
                _logger.LogInformation("Sending notification request: {@Request}", request);

                var response = await _httpClient.PostAsJsonAsync("notifications/send", request);
                var responseData = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("[DEBUG] API Response: {Response}", responseData);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("[ERROR] Không thể gửi thông báo. Status: {StatusCode}, Response: {Response}",
                        response.StatusCode, responseData);
                    return false;
                }

                return true;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "[ERROR] Lỗi kết nối đến NotificationService.");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ERROR] Lỗi không xác định khi gửi thông báo.");
                return false;
            }
        }
    }

}
