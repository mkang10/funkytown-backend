using Application.Interfaces;
using AutoMapper;
using Domain.Common_Model;
using Domain.DTO.Response;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class GetAllReturnRequestsHandler
    {
        private readonly IReturnOrderRepository _returnOrderRepository;
        private readonly GetOrderDetailHandler _getOrderDetailHandler;  
        private readonly IOrderRepository _orderRepository;
        private readonly IInventoryServiceClient _inventoryServiceClient;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetAllReturnRequestsHandler> _logger;
        public GetAllReturnRequestsHandler(
            IReturnOrderRepository returnOrderRepository,
            GetOrderDetailHandler getOrderDetailHandler,   
            IOrderRepository orderRepository,
            IPaymentRepository paymentRepository,
            IInventoryServiceClient inventoryServiceClient,
            IMapper mapper

        )
        {
            _returnOrderRepository = returnOrderRepository;
            _getOrderDetailHandler = getOrderDetailHandler;
            _orderRepository = orderRepository;
            _paymentRepository = paymentRepository;
            _inventoryServiceClient = inventoryServiceClient;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<ReturnRequestWrapper>> HandleAsync(
                                                                string? status,
                                                                string? returnOption,
                                                                DateTime? dateFrom,
                                                                DateTime? dateTo,
                                                                int? orderId,
                                                                int? returnOrderId,
                                                                int handledBy,
                                                                int pageNumber,
                                                                int pageSize)
        {
            var pagedRo = await _returnOrderRepository.GetReturnOrdersAsync(
                status,
                returnOption,
                dateFrom,
                dateTo,
                orderId,
                returnOrderId,
                handledBy,
                pageNumber,
                pageSize);

            var wrapperList = new List<ReturnRequestWrapper>();

            foreach (var ro in pagedRo.Items)
            {
                var orderDetail = await GetOrderDetailForStaffAsync(ro.OrderId);

                // Parse ảnh từ JSON
                List<string>? images = null;
                if (!string.IsNullOrEmpty(ro.ReturnImages))
                {
                    images = JsonConvert.DeserializeObject<List<string>>(ro.ReturnImages);
                }

                // Map ReturnOrderInfo + ReturnOrderDetailInfo
                var returnOrderInfo = _mapper.Map<ReturnOrderInfo>(ro);
                returnOrderInfo.AccountName = orderDetail?.FullName ?? "Không xác định";

                var returnOrderDetailInfo = _mapper.Map<ReturnOrderDetailInfo>(ro);
                returnOrderDetailInfo.ReturnImages = images;

                // Map ReturnItems → ReturnOrderItemInfo
                var returnItems = ro.ReturnOrderItems;
                var returnOrderItemInfos = new List<ReturnOrderItemInfo>();

                var variantIds = returnItems.Select(i => i.ProductVariantId).Distinct().ToList();
                var variantDict = await _inventoryServiceClient.GetAllProductVariantsByIdsAsync(variantIds);

                foreach (var item in returnItems)
                {
                    if (variantDict.TryGetValue(item.ProductVariantId, out var variant))
                    {
                        returnOrderItemInfos.Add(new ReturnOrderItemInfo
                        {
                            ProductVariantName = variant.ProductName,
                            Size = variant.Size,
                            Color = variant.Color,
                            ImageUrl = variant.ImagePath,
                            Quantity = item.Quantity,
                            Price = variant.Price,
                            PriceAtPurchase = orderDetail?.OrderItems
                                .FirstOrDefault(x => x.ProductVariantId == item.ProductVariantId)?.PriceAtPurchase ?? 0,
                            ShippingCost = orderDetail?.ShippingCost ?? 0
                        });
                    }
                }

                wrapperList.Add(new ReturnRequestWrapper
                {
                    ReturnOrder = returnOrderInfo,
                    ReturnOrderDetail = returnOrderDetailInfo,
                    ReturnOrderItems = returnOrderItemInfos
                });
            }

            return new PaginatedResult<ReturnRequestWrapper>(
                items: wrapperList,
                totalCount: pagedRo.TotalCount,
                pageNumber: pagedRo.PageNumber,
                pageSize: pagedRo.PageSize
            );
        }


        private async Task<OrderDetailResponseWrapper?> GetOrderDetailForStaffAsync(int orderId)
        {
            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                return null;
            }

            var orderItemsResponses = _mapper.Map<List<OrderItemResponse>>(order.OrderDetails);
            var variantIds = orderItemsResponses.Select(d => d.ProductVariantId).Distinct().ToList();

            Dictionary<int, ProductVariantResponse> variantDetailsDict = new();
            try
            {
                variantDetailsDict = await _inventoryServiceClient.GetAllProductVariantsByIdsAsync(variantIds);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[StaffMode] Error fetching variant details: {ex.Message}");
            }

            foreach (var detail in orderItemsResponses)
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
                OrderItems = orderItemsResponses,
                Status = order.Status,
                CreatedDate = order.CreatedDate,
                Ghnid = order.Ghnid
            };
        }

    }

}
