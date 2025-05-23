
using Application.Interfaces;
using AutoMapper;
using Domain.DTO.Response;
using Domain.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class GetOrderItemsForReturnHandler
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IInventoryServiceClient _inventoryServiceClient;
        private readonly IMapper _mapper;
        private readonly IDistributedCache _cache;
        private readonly ILogger<GetOrderItemsForReturnHandler> _logger;

        public GetOrderItemsForReturnHandler(
            IOrderRepository orderRepository,
            IInventoryServiceClient inventoryServiceClient,
            IMapper mapper,
            IDistributedCache cache,
            ILogger<GetOrderItemsForReturnHandler> logger)
        {
            _orderRepository = orderRepository;
            _inventoryServiceClient = inventoryServiceClient;
            _mapper = mapper;
            _cache = cache;
            _logger = logger;
        }

        public async Task<List<OrderItemResponse>> Handle(int orderId, int accountId)
        {
            string instanceName = "OrderInstance:";
            var cacheKey = $"{instanceName}ReturnOrderItems_{accountId}_{orderId}";

            // Kiểm tra nếu đã có trong Redis
            var cachedData = await _cache.GetAsync(cacheKey);
            if (cachedData != null)
            {
                var json = Encoding.UTF8.GetString(cachedData);
                return JsonConvert.DeserializeObject<List<OrderItemResponse>>(json) ?? new List<OrderItemResponse>();
            }

            // Nếu không có cache, lấy từ DB
            var order = await _orderRepository.GetOrderWithDetailsAsync(orderId);
            if (order == null || order.OrderDetails == null || !order.OrderDetails.Any())
            {
                return new List<OrderItemResponse>();
            }

            var orderItemResponses = _mapper.Map<List<OrderItemResponse>>(order.OrderDetails);
            // Lấy danh sách ProductVariantId duy nhất
            var variantIds = orderItemResponses.Select(d => d.ProductVariantId).Distinct().ToList();
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

            // Ánh xạ thông tin sản phẩm
            foreach (var detail in orderItemResponses)
            {
                if (variantDetailsDict.TryGetValue(detail.ProductVariantId, out var variantDetails))
                {

                    detail.ProductId = variantDetails.ProductId;
                    detail.ProductName = variantDetails.ProductName;
                    detail.Color = variantDetails.Color;
                    detail.Size = variantDetails.Size;
                    detail.ImageUrl = variantDetails.ImagePath;
                }
            }

            // Lưu vào Redis với thời gian 30 phút
            var jsonData = JsonConvert.SerializeObject(orderItemResponses);
            await _cache.SetAsync(cacheKey, Encoding.UTF8.GetBytes(jsonData), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
            });

            return orderItemResponses;
        }
    }

}
