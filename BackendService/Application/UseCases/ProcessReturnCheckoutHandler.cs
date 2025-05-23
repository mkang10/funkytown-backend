
using Application.Interfaces;
using AutoMapper;
using Domain.DTO.Request;
using Domain.DTO.Response;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class ProcessReturnCheckoutHandler
    {
        private readonly IDistributedCache _cache;
        private readonly IMapper _mapper;

        public ProcessReturnCheckoutHandler(
            IDistributedCache cache,
            IMapper mapper)
        {
            _cache = cache;
            _mapper = mapper;
        }

        public async Task<ReturnCheckoutResponse> Handle(ReturnCheckOutRequest request)
        {
            var returnCheckoutSessionId = Guid.NewGuid().ToString(); // ✅ Tạo Session ID
            string instanceName = "OrderInstance:";
            var cacheKey = $"{instanceName}ReturnOrderItems_{request.AccountId}_{request.OrderId}";
            var cachedData = await _cache.GetAsync(cacheKey);
            if (cachedData == null)
            {
                return null;
            }

            var json = Encoding.UTF8.GetString(cachedData);
            var allOrderItems = JsonConvert.DeserializeObject<List<OrderItemResponse>>(json) ?? new List<OrderItemResponse>();

            // ✅ Lọc danh sách sản phẩm theo danh sách ID đã chọn
            var selectedOrderItems = allOrderItems
                .Where(o => request.SelectedItems.Any(s => s.ProductVariantId == o.ProductVariantId))
                .Select(o =>
                {
                    var selectedItem = request.SelectedItems.First(s => s.ProductVariantId == o.ProductVariantId);
                    return new ReturnItemResponse
                    {
                        ProductVariantId = o.ProductVariantId,
                        ProductName = o.ProductName,
                        Color = o.Color,
                        Size = o.Size,
                        ImageUrl = o.ImageUrl,
                        Quantity = selectedItem.Quantity, // ✅ Chỉ lấy số lượng khách hàng muốn trả
                        Price = o.PriceAtPurchase
                    };
                }).ToList();

            if (!selectedOrderItems.Any())
            {
                return null;
            }

            decimal totalRefundAmount = selectedOrderItems.Sum(item => item.Price * item.Quantity);
           

            // ✅ Lưu vào Redis để giữ phiên đổi trả trong 15 phút
            var returnCheckoutData = new ReturnCheckoutData
            {
                AccountId = request.AccountId,
                OrderId = request.OrderId,
                TotalRefundAmount = totalRefundAmount,
                Items = selectedOrderItems
            };

            var returnCacheKey = $"return-checkout:{returnCheckoutSessionId}";
            var cacheOptions = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15) };
            await _cache.SetStringAsync(returnCacheKey, JsonConvert.SerializeObject(returnCheckoutData), cacheOptions);

            return new ReturnCheckoutResponse
            {
                ReturnCheckoutSessionId = returnCheckoutSessionId,
                OrderId = request.OrderId,
                AccountId = request.AccountId,
                ReturnItems = selectedOrderItems, // ✅ Chỉ chứa thông tin sản phẩm mà KHÁCH HÀNG ĐÃ CHỌN
                TotalRefundAmount = totalRefundAmount, // ✅ Hiển thị tổng tiền có thể hoàn
                RefundMethods = new List<string> { "Hoàn tiền qua ngân hàng" },
                ReturnReasons = new List<string>
                {
                    "Sản phẩm bị lỗi",
                    "Sản phẩm không đúng mô tả",
                    "Sai kích thước/màu sắc",
                    "Không hài lòng về chất lượng",
                    "Lý do khác"
                }, 
                ReturnOptions = new List<string>
                {
                    "Đổi hàng",
                    "Hoàn tiền"
                }, 
                ReturnDescription = "", // ✅ Người dùng sẽ nhập lý do đổi trả chi tiết
                MediaUrls = new List<string>(), // ✅ Người dùng có thể đính kèm hình ảnh/video
                Email = ""
            };
        }


    }

}
