using Application.Interfaces;
using AutoMapper;
using Domain.DTO.Response;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class GetReturnableOrdersHandler
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;
        private readonly IInventoryServiceClient _inventoryServiceClient;
        private readonly IAuditLogRepository _auditLogRepository;
        public GetReturnableOrdersHandler(IOrderRepository orderRepository, IMapper mapper, IInventoryServiceClient inventoryServiceClient, IAuditLogRepository auditLogRepository)
        {
            _orderRepository = orderRepository;
            _mapper = mapper;
            _inventoryServiceClient = inventoryServiceClient;
            _auditLogRepository = auditLogRepository;
        }

        public async Task<List<OrderResponse>> HandleAsync(int accountId)
        {
            // 📌 1️⃣ Lấy danh sách đơn hàng có thể đổi trả
            var orders = await _orderRepository.GetReturnableOrdersAsync(accountId);
            if (!orders.Any()) return new List<OrderResponse>();

            var orderResponses = _mapper.Map<List<OrderResponse>>(orders);

            // 📌 2️⃣ Lấy tất cả `ProductVariantId` một lần để tối ưu API call
            var productVariantIds = orders
                .SelectMany(o => o.OrderDetails.Select(i => i.ProductVariantId))
                .Distinct()
                .ToList();

            var variantDetailsMap = await _inventoryServiceClient.GetAllProductVariantsByIdsAsync(productVariantIds);

            // 📌 3️⃣ Cập nhật thông tin vào `OrderResponse`
            await Parallel.ForEachAsync(orderResponses, async (orderResponse, _) =>
            {
                foreach (var item in orderResponse.Items)
                {
                    if (variantDetailsMap.TryGetValue(item.ProductVariantId, out var variantDetails))
                    {
                        item.ProductName = variantDetails.ProductName;
                        item.Color = variantDetails.Color;
                        item.Size = variantDetails.Size;
                        item.ImageUrl = variantDetails.ImagePath;
                    }
                }
            });

            return orderResponses;
        }
        public async Task<bool> CheckReturnableHandleAsync(int orderId, int accountId)
        {
            return await _orderRepository.IsOrderReturnableAsync(orderId, accountId);
        }
    }

}
