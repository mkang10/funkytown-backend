
using Application.Interfaces;
using Domain.DTO.Request;
using Domain.DTO.Response;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class SubmitReturnRequestHandler
    {
        private readonly IDistributedCache _cache;
        private readonly IReturnOrderRepository _returnOrderRepository;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IOrderProcessingHelper _orderProcessingHelper;
        private readonly UpdateOrderStatusHandler _updateOrderStatusHandler;
        private readonly IAssignmentSettingService _assignmentSettingService;
        public SubmitReturnRequestHandler(IDistributedCache cache, IReturnOrderRepository returnOrderRepository,
                                            ICloudinaryService cloudinaryService, IOrderProcessingHelper orderProcessingHelper,
                                            UpdateOrderStatusHandler updateOrderStatusHandler, IAssignmentSettingService assignmentSettingService)
        {
            _cache = cache;
            _returnOrderRepository = returnOrderRepository;
            _cloudinaryService = cloudinaryService;
            _orderProcessingHelper = orderProcessingHelper;
            _updateOrderStatusHandler = updateOrderStatusHandler;
            _assignmentSettingService = assignmentSettingService;
        }

        public async Task<SubmitReturnResponse?> Handle(SubmitReturnRequest request)
        {
            var cacheKey = $"return-checkout:{request.ReturnCheckoutSessionId}";
            var cachedData = await _cache.GetAsync(cacheKey);
            if (cachedData == null) return null;

            var json = Encoding.UTF8.GetString(cachedData);
            var returnCheckoutData = JsonConvert.DeserializeObject<ReturnCheckoutData>(json);
            if (returnCheckoutData == null || !returnCheckoutData.Items.Any()) return null;

            // ✅ 1️⃣ Tải hình ảnh/video lên Cloudinary và lưu danh sách URL
            var mediaUrls = new List<string>();
            foreach (var file in request.MediaFiles)
            {
                var mediaUrl = await _cloudinaryService.UploadMediaAsync(file);
                if (!string.IsNullOrEmpty(mediaUrl))
                {
                    mediaUrls.Add(mediaUrl);
                }
            }
            var shopManagerId = _assignmentSettingService.DefaultShopManagerId;

            // ✅ 2️⃣ Tạo đối tượng `ReturnOrder`
            var returnOrder = new ReturnOrder
            {
                OrderId = returnCheckoutData.OrderId,
                AccountId = returnCheckoutData.AccountId,
                Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email,
                TotalRefundAmount = returnCheckoutData.TotalRefundAmount,
                ReturnReason = request.ReturnReason,
                ReturnOption = request.ReturnOption,
                RefundMethod = request.RefundMethod,
                ReturnDescription = request.ReturnDescription,
                Status = "Pending Processing",
                CreatedDate = DateTime.UtcNow,
                ReturnImages = mediaUrls.Any() ? JsonConvert.SerializeObject(mediaUrls) : null, // ✅ Lưu URL ảnh/video vào JSON
                HandledBy = shopManagerId
            };

            // ✅ 3️⃣ Nếu chọn phương thức hoàn tiền qua ngân hàng, lưu thông tin ngân hàng
            var refundMethodLower = request.RefundMethod.Trim().ToLower();
            if (refundMethodLower == "hoàn tiền qua ngân hàng")
            {
                returnOrder.BankName = request.BankName;
                returnOrder.BankAccountNumber = request.BankAccountNumber;
                returnOrder.BankAccountName = request.BankAccountName;
            }

            // ✅ 4️⃣ Lưu đơn đổi trả vào DB
            await _returnOrderRepository.CreateReturnOrderAsync(returnOrder);
            await _orderProcessingHelper.LogPendingReturnStatusAsync(returnOrder.ReturnOrderId, returnOrder.AccountId);
            await _orderProcessingHelper.SendReturnOrderNotificationAsync(
                                    returnOrder.AccountId,
                                    returnOrder.ReturnOrderId,
                                    "Yêu cầu đổi trả ",
                                    $"Yêu cầu đổi trả #{returnOrder.ReturnOrderId} đã được tạo thành công và đang chờ xử lí ."
                                );
            await _orderProcessingHelper.AssignReturnOrderToManagerAsync(orderId: returnOrder.OrderId, assignedBy: returnOrder.AccountId);
            // ✅ 5️⃣ Lưu danh sách sản phẩm đổi trả vào `ReturnOrderItem`
            var returnOrderItems = returnCheckoutData.Items.Select(item => new ReturnOrderItem
            {
                ReturnOrderId = returnOrder.ReturnOrderId,
                ProductVariantId = item.ProductVariantId,
                Quantity = item.Quantity,
                RefundPrice = item.Price * item.Quantity // ✅ Tính số tiền hoàn lại cho sản phẩm
            }).ToList();

            await _returnOrderRepository.AddReturnOrderItemsAsync(returnOrderItems);
            await _updateOrderStatusHandler.HandleAsync(
                orderId: returnOrder.OrderId,
                newStatus: "Return Requested", // hoặc "Đang chờ xử lý đổi trả"
                changedBy: returnOrder.AccountId,
                comment: $"Yêu cầu đổi trả #{returnOrder.ReturnOrderId} đã được gửi."
            );
            // ✅ 6️⃣ Xóa dữ liệu cache sau khi hoàn tất
            await _cache.RemoveAsync(cacheKey);

            return new SubmitReturnResponse
            {
                ReturnOrderId = returnOrder.ReturnOrderId,
                Status = "Pending Processing",
            };
        }

    }


}
