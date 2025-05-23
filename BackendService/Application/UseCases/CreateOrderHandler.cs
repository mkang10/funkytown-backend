
using Application.Interfaces;
using AutoMapper;
using Domain.DTO.Request;
using Domain.DTO.Response;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class CreateOrderHandler
    {
        private readonly IDistributedCache _cache;
        private readonly IOrderRepository _orderRepository;
        private readonly ICustomerServiceClient _customerServiceClient;
        private readonly IInventoryServiceClient _inventoryServiceClient;
        private readonly IPayOSService _payOSService;
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;
        private readonly GetShippingAddressHandler _getShippingAddressHandler;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateOrderHandler> _logger;
        private readonly ShippingCostHandler _shippingCostHandler;
        private readonly AuditLogHandler _auditLogHandler;
        private readonly INotificationClient _notificationClient;
        private readonly IOrderProcessingHelper _orderHelper;
        private readonly EmailHandler _emailService;
        public CreateOrderHandler(
            IDistributedCache cache,
            IOrderRepository orderRepository,
            ICustomerServiceClient customerServiceClient,
            IInventoryServiceClient inventoryServiceClient,
            IPayOSService payOSService,
            IConfiguration configuration,
            IUnitOfWork unitOfWork,
            GetShippingAddressHandler getShippingAddressHandler,
            IMapper mapper,
            ShippingCostHandler shippingCostHandler,
            ILogger<CreateOrderHandler> logger,
            AuditLogHandler auditLogHandler,
            INotificationClient notificationClient,
            IOrderProcessingHelper orderHelper,
            EmailHandler emailService)
        {
            _cache = cache;
            _mapper = mapper;
            _orderRepository = orderRepository;
            _customerServiceClient = customerServiceClient;
            _inventoryServiceClient = inventoryServiceClient;
            _payOSService = payOSService;
            _configuration = configuration;
            _unitOfWork = unitOfWork;
            _getShippingAddressHandler = getShippingAddressHandler;
            _shippingCostHandler = shippingCostHandler;
            _logger = logger;
            _auditLogHandler = auditLogHandler;
            _notificationClient = notificationClient;
            _orderHelper = orderHelper;
            _emailService = emailService;
        }

        public async Task<OrderResponse?> Handle(CreateOrderRequest request)
        {
            // Bắt đầu transaction
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                // Lấy thông tin phiên checkout từ Redis bằng CheckOutSessionId
                var cacheKey = $"checkout:{request.CheckOutSessionId}";
                var checkoutDataJson = await _cache.GetStringAsync(cacheKey);
                if (string.IsNullOrEmpty(checkoutDataJson))
                {
                    await _unitOfWork.RollbackAsync();
                    return null; // Hoặc có thể fallback lấy thông tin từ các service
                }
                var checkoutData = JsonConvert.DeserializeObject<CheckOutData>(checkoutDataJson);
                if (checkoutData == null)
                {
                    await _unitOfWork.RollbackAsync();
                    return null;
                }

                // Xác định ShippingAddressId cần dùng:
                int shippingAddressId = request.ShippingAddressId
                        ?? checkoutData.ShippingAddressId
                        ?? throw new InvalidOperationException("Shipping address is required.");


                // Lấy thông tin địa chỉ giao hàng dựa trên shippingAddressId được xác định
                var shippingAddress = await _getShippingAddressHandler.HandleAsync(shippingAddressId, request.AccountId);
                if (shippingAddress == null)
                {
                    await _unitOfWork.RollbackAsync();
                    return null;
                }

                decimal shippingCost = checkoutData.ShippingCost;

                // Lấy lại danh sách sản phẩm đã chọn từ phiên checkout (để đảm bảo số lượng, giá cả chưa thay đổi)
                var orderItems = checkoutData.Items;
                if (orderItems == null || !orderItems.Any())
                {
                    await _unitOfWork.RollbackAsync();
                    return null;
                }

                // Lấy tổng tiền đã tính từ phiên checkout
                var subTotal = checkoutData.SubTotal;
                if (subTotal <= 0)
                {
                    await _unitOfWork.RollbackAsync();
                    return null;
                }

                // Lấy WarehouseId từ appsettings.json, nếu không có thì dùng giá trị mặc định
                int warehouseId = int.TryParse(_configuration["Warehouse:OnlineWarehouseId"], out var wId) ? wId : 2;


                // Tạo Order và copy snapshot thông tin từ ShippingAddress
                var newOrder = _mapper.Map<Order>(shippingAddress);
                newOrder.AccountId = request.AccountId;
                newOrder.CreatedDate = DateTime.UtcNow;
                newOrder.OrderTotal = subTotal;
                newOrder.ShippingCost = shippingCost;
                newOrder.ShippingAddressId = shippingAddress.AddressId;

                // Thiết lập trạng thái ban đầu tùy theo phương thức thanh toán (sẽ cập nhật sau)
                newOrder.Status = request.PaymentMethod == "PAYOS" ? "Pending Payment" : "Pending Confirmed";
                newOrder.WareHouseId = warehouseId;

                // Lưu Order (chưa commit)
                await _orderRepository.CreateOrderAsync(newOrder);
                await _unitOfWork.SaveChangesAsync();

                // Tạo OrderDetails từ orderItems
                var orderDetails = CreateOrderDetails(newOrder, orderItems);

                // Xử lý theo PaymentMethod
                if (request.PaymentMethod == "PAYOS")
                {
                    // Gọi PayOS tạo link thanh toán
                    
                    var payosInfraResult = await _payOSService.CreatePayment(newOrder.OrderId, subTotal + shippingCost, request.PaymentMethod, orderItems);
                    if (payosInfraResult == null || string.IsNullOrEmpty(payosInfraResult.CheckoutUrl))
                    {
                        await _unitOfWork.RollbackAsync();
                        return null;
                    }

                    // Map sang CreatePaymentResult bằng AutoMapper
                    var paymentResult = _mapper.Map<CreatePaymentResponse>(payosInfraResult);

                    // Tạo đối tượng Payment (chưa commit)
                    await _orderHelper.SavePaymentAndOrderDetailsAsync(newOrder, orderDetails, request.PaymentMethod, subTotal, shippingCost, paymentResult.OrderCode);

                    // Sau khi đặt hàng thành công, xóa sản phẩm khỏi giỏ hàng
                    await _orderHelper.ClearCartAsync(request.AccountId, orderItems.Select(i => i.ProductVariantId).ToList());

                    // Ghi lại lịch sử đơn hàng
                    await _orderHelper.LogPendingPaymentStatusAsync(newOrder.OrderId, request.AccountId);

                    await _orderHelper.SendOrderNotificationAsync(
                                    newOrder.AccountId,
                                    newOrder.OrderId,
                                    "Đơn hàng mới",
                                    $"Đơn hàng #{newOrder.OrderId} đã được tạo thành công và đang chờ thanh toán."
                                );
                    // Commit transaction
                    await _unitOfWork.CommitAsync();
                    await _emailService.InvoiceForEmail(newOrder.OrderId);
                    return _orderHelper.BuildOrderResponse(newOrder, request.PaymentMethod, paymentResult.CheckoutUrl);
                }
                else if (request.PaymentMethod == "COD")
                {
                    await _orderHelper.SavePaymentAndOrderDetailsAsync(newOrder, orderDetails, request.PaymentMethod, subTotal, shippingCost);
                    // Cập nhật tồn kho ngay lập tức
                    var updateStockSuccess = await _inventoryServiceClient.UpdateStockAfterOrderAsync(warehouseId, orderDetails);
                    if (!updateStockSuccess)
                    {
                        await _unitOfWork.RollbackAsync();
                        return null;
                    }
                    await _orderHelper.LogWarehouseStockChangeAsync(newOrder.OrderId, request.AccountId, orderDetails, warehouseId);


                    await _orderHelper.ClearCartAsync(request.AccountId, orderItems.Select(i => i.ProductVariantId).ToList());

                    // Ghi lại lịch sử đơn hàng
                    await _orderHelper.LogPendingConfirmedStatusAsync(newOrder.OrderId, request.AccountId);
                    await _orderHelper.AssignOrderToManagerAsync(orderId: newOrder.OrderId, assignedBy: request.AccountId);

                    await _orderHelper.SendOrderNotificationAsync(
                                        newOrder.AccountId,
                                        newOrder.OrderId,
                                        "Đơn hàng mới",
                                        $"Đơn hàng #{newOrder.OrderId} đã được tạo thành công và đang chờ xác nhận."
                                    );

                    await _unitOfWork.CommitAsync();
                    await _emailService.InvoiceForEmail(newOrder.OrderId);
                    return _orderHelper.BuildOrderResponse(newOrder, request.PaymentMethod);
                }

                await _unitOfWork.RollbackAsync();
                return null;
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }


        ///////////////////////////////////////////////////////
        private List<OrderDetail> CreateOrderDetails(Order newOrder, List<OrderItemRequest> orderItems)
        {
            return orderItems.Select(item => new OrderDetail
            {
                OrderId = newOrder.OrderId,
                ProductVariantId = item.ProductVariantId,
                Quantity = item.Quantity,
                PriceAtPurchase = item.DiscountedPrice,
                DiscountApplied = item.Price - item.DiscountedPrice
            }).ToList();
        }

        /// 📌 Tính tổng tiền đơn hàng
        private async Task<decimal> CalculateTotalAmountAsync(List<OrderItemRequest> cartItems)
        {
            decimal totalAmount = 0;

            foreach (var item in cartItems)
            {
                var productVariant = await _inventoryServiceClient.GetProductVariantByIdAsync(item.ProductVariantId);
                if (productVariant == null)
                    return -1; // Nếu không tìm thấy sản phẩm, trả về lỗi

                totalAmount += productVariant.Price * item.Quantity;
            }

            return totalAmount;
        }

        /// 📌 Tạo đơn hàng trong DB và lưu OrderDetails 
        private async Task<Order> CreateOrderAsync(CreateOrderRequest request, decimal totalAmount, decimal shippingCost, List<OrderItemRequest> orderItems, ShippingAddress shippingAddress)
        {
            var order = new Order
            {
                AccountId = request.AccountId,
                CreatedDate = DateTime.UtcNow,
                Status = "Pending",
                OrderTotal = totalAmount,
                ShippingCost = shippingCost,
                ShippingAddressId = shippingAddress.AddressId, // Lưu liên kết đến ShippingAddress

                // Snapshot thông tin giao hàng từ ShippingAddress
                FullName = shippingAddress.RecipientName,
                Email = shippingAddress.Email ?? string.Empty,
                PhoneNumber = shippingAddress.RecipientPhone,
                Address = shippingAddress.Address,
                City = shippingAddress.City ?? string.Empty,
                District = shippingAddress.District ?? string.Empty,
                Country = shippingAddress.Country,
                Province = shippingAddress.Province
            };

            await _orderRepository.CreateOrderAsync(order);

            // Lưu danh sách sản phẩm (OrderDetails)
            var orderDetails = orderItems.Select(item => new OrderDetail
            {
                OrderId = order.OrderId,
                ProductVariantId = item.ProductVariantId,
                Quantity = item.Quantity,
                PriceAtPurchase = item.Price,
                DiscountApplied = 0
            }).ToList();

            await _orderRepository.SaveOrderDetailsAsync(orderDetails);

            return order;
        }

    }
}
