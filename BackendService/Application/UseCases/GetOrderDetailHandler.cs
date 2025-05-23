
using Application.Interfaces;
using AutoMapper;
using Domain.DTO.Response;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class GetOrderDetailHandler
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IInventoryServiceClient _inventoryServiceClient;
        private readonly IMapper _mapper;
        private readonly ILogger<GetOrderDetailHandler> _logger;

        public GetOrderDetailHandler(
            IOrderRepository orderRepository,
            IPaymentRepository paymentRepository,
            IInventoryServiceClient inventoryServiceClient,
            IMapper mapper,
            ILogger<GetOrderDetailHandler> logger)
        {
            _orderRepository = orderRepository;
            _paymentRepository = paymentRepository;
            _inventoryServiceClient = inventoryServiceClient;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<OrderDetailResponseWrapper?> HandleAsync(int orderId, int accountId)
        {
            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            if (order == null || order.AccountId != accountId)
            {
                return null; // Không tìm thấy hoặc không thuộc về accountId
            }

            var orderitemsResponses = _mapper.Map<List<OrderItemResponse>>(order.OrderDetails);
            var variantIds = orderitemsResponses.Select(d => d.ProductVariantId).Distinct().ToList();

            Dictionary<int, ProductVariantResponse> variantDetailsDict = new();
            try
            {
                variantDetailsDict = await _inventoryServiceClient.GetAllProductVariantsByIdsAsync(variantIds);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching product/store details: {ex.Message}");
            }

            foreach (var detail in orderitemsResponses)
            {
                if (variantDetailsDict.TryGetValue(detail.ProductVariantId, out var variantDetails))
                {
                    detail.ProductId = variantDetails.ProductId;
                    detail.ProductName = variantDetails.ProductName;
                    detail.Color = variantDetails.Color;
                    detail.Size = variantDetails.Size;
                    detail.ImageUrl = variantDetails.ImagePath;
                    detail.Price = variantDetails.Price;
                    detail.DiscountApplied = variantDetails.DiscountedPrice;
                }
                else
                {
                    detail.ProductId = 0;
                    detail.ProductName = "Không xác định";
                    detail.Color = "Không xác định";
                    detail.Size = "Không xác định";
                    detail.ImageUrl = "Không xác định";
                    detail.Price = 0;
                    detail.DiscountApplied = 0;
                }
            }

            var paymentMethod = await _paymentRepository.GetPaymentMethodByOrderIdAsync(orderId) ?? "Không xác định";

            return new OrderDetailResponseWrapper
            {
                OrderId = order.OrderId,
                FullName = order.FullName,
                Email = order.Email,
                PhoneNumber = order.PhoneNumber,
                Address = order.Address,
                City = order.City,
                District = order.District,
                Province = order.Province,
                Country = order.Country,
                PaymentMethod = paymentMethod,
                OrderTotal = order.OrderTotal ?? 0,
                ShippingCost = order.ShippingCost ?? 0,
                OrderItems = orderitemsResponses,
                Status = order.Status,
                CreatedDate = order.CreatedDate,
                Ghnid = order.Ghnid,
                IsFeedback = order.IsFeedback,
                CompletedDate = order.CompletedDate,
            };
        }

    }


}
