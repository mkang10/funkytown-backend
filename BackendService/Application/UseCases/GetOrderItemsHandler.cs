
using Application.Interfaces;
using AutoMapper;
using Domain.DTO.Response;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class GetOrderItemsHandler
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IInventoryServiceClient _inventoryServiceClient;
        private readonly IMapper _mapper;
        private readonly ILogger<GetOrderItemsHandler> _logger;

        public GetOrderItemsHandler(
            IOrderRepository orderRepository,
            IInventoryServiceClient inventoryServiceClient,
            IMapper mapper,
            ILogger<GetOrderItemsHandler> logger)
        {
            _orderRepository = orderRepository;
            _inventoryServiceClient = inventoryServiceClient;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<List<OrderItemResponse>> HandleAsync(int orderId)
        {
            // 1️⃣ Lấy thông tin đơn hàng từ DB
            var order = await _orderRepository.GetOrderWithDetailsAsync(orderId);
            if (order == null || order.OrderDetails == null || !order.OrderDetails.Any())
            {
                return new List<OrderItemResponse>(); // Trả về danh sách rỗng thay vì null
            }

            var orderItemResponses = _mapper.Map<List<OrderItemResponse>>(order.OrderDetails);

            // 2️⃣ Lấy danh sách ProductVariantId duy nhất
            var variantIds = orderItemResponses.Select(d => d.ProductVariantId).Distinct().ToList();

            // 3️⃣ Gửi request lấy thông tin Product Variants từ Inventory Service
            Dictionary<int, ProductVariantResponse> variantDetailsDict = new();
            try
            {
                if (variantIds.Any())
                {
                    variantDetailsDict = await _inventoryServiceClient.GetAllProductVariantsByIdsAsync(variantIds);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi khi lấy thông tin sản phẩm từ InventoryService: {ex.Message}");
            }

            // 4️⃣ Ánh xạ thông tin sản phẩm vào OrderItemResponse
            foreach (var detail in orderItemResponses)
            {
                if (variantDetailsDict.TryGetValue(detail.ProductVariantId, out var variantDetails))
                {
                    detail.ProductName = variantDetails.ProductName;
                    detail.Color = variantDetails.Color;
                    detail.Size = variantDetails.Size;
                    detail.ImageUrl = variantDetails.ImagePath;
                }
                else
                {
                    detail.ProductName = "Không xác định";
                    detail.Color = "Không xác định";
                    detail.Size = "Không xác định";
                    detail.ImageUrl = "Không xác định";
                }
            }

            return orderItemResponses;
        }
    }
}
