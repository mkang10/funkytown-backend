using Application.Interfaces;
using AutoMapper;
using Domain.DTO.Request;
using Domain.DTO.Response;
using Infrastructure.HelperServices.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Net.payOS;
using Net.payOS.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.HelperServices
{
    public class PayOSService : IPayOSService
    {
        private readonly PayOS _payOS;
        private readonly HttpClient _httpClient;
        private readonly ILogger<PayOSService> _logger;
        private readonly IMapper _mapper;
        public PayOSService(IConfiguration configuration, HttpClient httpClient, ILogger<PayOSService> logger, IMapper mapper)
        {

            string apiKey = configuration["PayOS:ApiKey"];
            string clientId = configuration["PayOS:ClientId"];
            string checksumKey = configuration["PayOS:ChecksumKey"];
            string webhookUrl = configuration["PayOS:WebhookUrl"];

            _payOS = new PayOS(clientId, apiKey, checksumKey);
            _httpClient = httpClient;
            _logger = logger;
            _mapper = mapper;
            try
            {
                _payOS.confirmWebhook(webhookUrl);
                _logger.LogInformation($"Webhook URL registered successfully: {webhookUrl}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to register Webhook URL with payOS");
            }

        }

        public async Task<CreatePaymentResponse?> CreatePayment(int orderId, decimal amount, string paymentMethod, List<OrderItemRequest> orderItems)
        {
            var items = orderItems
                     .Select(i => new ItemData(
                         name: $"{i.ProductName} ({i.Color}, {i.Size})",
                         quantity: i.Quantity,
                         price: Convert.ToInt32(i.Price)
                     ))
                     .ToList();

            // Thêm dòng tổng quan
            items.Add(new ItemData(
                name: $"Đơn hàng #{orderId}",
                quantity: 1,
                price: Convert.ToInt32(amount)
            ));

        
            long orderCode = long.Parse($"{orderId}{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() % 1000000}");

            var paymentData = new PaymentData(
                orderCode: orderCode,
                amount: (int)amount,
                description: $"Thanh toán đơn hàng {orderId}",
                items: items,
                cancelUrl: "https://ftorderserviceapi.azurewebsites.net/api/payment/cancel",
                returnUrl: $"http://localhost:3000/profile/order"
            );

            var createPayment = await _payOS.createPaymentLink(paymentData);
            var payOSCreateResult = new PayOSCreateResult
            {
                CheckoutUrl = createPayment.checkoutUrl,
                OrderCode = orderCode
            };

            // Dùng AutoMapper để map sang DTO trả về đúng theo interface
            return _mapper.Map<CreatePaymentResponse>(payOSCreateResult);
        }

        public async Task<PaymentLinkInformation?> GetPaymentStatus(int orderId)
        {
            return await _payOS.getPaymentLinkInformation(orderId);
        }

        public async Task<bool> CancelPayment(int orderId)
        {
            var cancelledPayment = await _payOS.cancelPaymentLink(orderId, "Hủy đơn hàng từ hệ thống");
            return cancelledPayment != null;
        }
    }
}
