
using Application.Interfaces;
using AutoMapper;
using Domain.Common_Model;
using Domain.DTO.Response;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class GetOrdersByStatusHandler
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;
        private readonly IInventoryServiceClient _inventoryServiceClient;

        public GetOrdersByStatusHandler(IOrderRepository orderRepository, IMapper mapper, IInventoryServiceClient inventoryServiceClient)
        {
            _orderRepository = orderRepository;
            _mapper = mapper;
            _inventoryServiceClient = inventoryServiceClient;
        }

        public async Task<PaginatedResult<OrderResponse>> HandleAsync(string? status, int? accountId, int pageNumber, int pageSize)
        {
            var paginatedOrders = await _orderRepository.GetOrdersByStatusPagedAsync(status, accountId, pageNumber, pageSize);
            var orderResponses = _mapper.Map<List<OrderResponse>>(paginatedOrders.Items);

            // ✅ 1. Lấy tất cả ProductVariantId từ toàn bộ đơn hàng
            var allVariantIds = orderResponses
                .SelectMany(o => o.Items)
                .Select(i => i.ProductVariantId)
                .Distinct()
                .ToList();

            // ✅ 2. Gọi batch API để lấy thông tin tất cả variant một lần
            var variantDict = await _inventoryServiceClient.GetAllProductVariantsByIdsAsync(allVariantIds);

            // ✅ 3. Gán thông tin variant vào từng item
            foreach (var order in orderResponses)
            {
                foreach (var item in order.Items)
                {
                    if (variantDict.TryGetValue(item.ProductVariantId, out var variant))
                    {
                        item.ProductName = variant.ProductName;
                        item.Color = variant.Color;
                        item.Size = variant.Size;
                        item.ImageUrl = variant.ImagePath;
                    }
                }
            }

            return new PaginatedResult<OrderResponse>(
                orderResponses,
                paginatedOrders.TotalCount,
                paginatedOrders.PageNumber,
                paginatedOrders.PageSize
            );
        }


    }

}
