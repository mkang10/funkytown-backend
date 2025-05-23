using Application.Enums;
using AutoMapper;
using Domain.DTO.Request;
using Domain.Entities;
using Domain.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class GHNLogHandler
    {

        private readonly IOrderRepository _orderRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IGHNLogRepository _ghNLogRepository;
        private readonly IMapper _mapper;

        public GHNLogHandler(IOrderRepository orderRepository, IAuditLogRepository auditLogRepository, IGHNLogRepository ghNLogRepository, IMapper mapper)
        {
            _orderRepository = orderRepository;
            _auditLogRepository = auditLogRepository;
            _ghNLogRepository = ghNLogRepository;
            _mapper = mapper;
        }

        public async Task<bool> AddGHNIdToOrderTableHandler(int id, UpdateGHNIdDTO user)
        {
            try
            {
                var userData = await _ghNLogRepository.GetOrderById(id);
                if (userData == null)
                {
                    throw new Exception("No data!");
                }
                _mapper.Map(user, userData);
                await _ghNLogRepository.AddGHNIdtoOrderTable(userData);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred: " + ex.Message);
            }
        }
        public async Task<bool> GetOrderByGHNId(string orderId, string newStatus)
        {
            // 📌 1️⃣ Lấy thông tin đơn hàng
            var order = await _orderRepository.GetOrderByIdGHNAsync(orderId);
            if (order == null)
                return false;

            // MỚI: nếu order đã ở trạng thái Completed thì không thay đổi nữa
            if (string.Equals(order.Status, "Completed", StringComparison.OrdinalIgnoreCase))
                return false;

            // 📌 2️⃣ Cập nhật trạng thái đơn hàng
            await _orderRepository.UpdateOrderStatusGHNIdAsync(orderId, newStatus);

            // 📌 3️⃣ Ghi log vào AuditLog
            var previousStatus = order.Status;
            var changeData = System.Text.Json.JsonSerializer.Serialize(new
            {
                OldStatus = previousStatus,
                NewStatus = newStatus
            });

            await _auditLogRepository.AddAuditLogAsync(
                "Orders",
                orderId.ToString(),
                AuditOperation.UpdateStatus.ToString(),  // 🔥 Sửa đây, luôn ghi UpdateStatus
                order.AccountId,
                changeData,
                "CHANGE STATUS"
            );


            return true;
        }

        public async Task<OrderRequest> AutoCreateOrderGHN(int id)
        {
            var dataModel = await _ghNLogRepository.GetDataOrder(id);

            var orderRequest = new OrderRequest
            {
                payment_type_id = 2,
                note = "Xin chao minh la FunkyTown Shop, rat vui duoc phuc vu ban",
                required_note = "KHONGCHOXEMHANG",
                from_name = dataModel.FirstOrDefault().Order.WareHouse.WarehouseName, // first or de fau chỉ dùng lấy 1 lần chứ kh gen ra 2 giá trị

                from_phone = dataModel.FirstOrDefault()?.Order.WareHouse.Phone,
                from_address = dataModel.FirstOrDefault()?.Order.WareHouse.Location,
                from_ward_name = "Phường 14",
                from_district_name = "Quận 10",
                from_province_name = "HCM",
                return_phone = dataModel.FirstOrDefault()?.Order.WareHouse.Phone,
                return_address = dataModel.FirstOrDefault()?.Order.WareHouse.Location,
                return_district_id = null,
                return_ward_code = "",
                client_order_code = "",
                to_name = dataModel.FirstOrDefault()?.Order.FullName,
                to_phone = dataModel.FirstOrDefault()?.Order.PhoneNumber,
                to_address = dataModel.FirstOrDefault()?.Order.ShippingAddress.Address,
                to_ward_name = "Phường Bến Nghé",
                to_ward_code = "",
                to_district_name = "Quận 1",
                to_province_name = "HCM",
                cod_amount = (int)dataModel.FirstOrDefault()?.Order.OrderTotal,
                content = "Funky Town",
                weight = 200,
                length = 1,
                width = 19,
                height = 10,
                pick_station_id = 1444,
                deliver_station_id = null,
                insurance_value = 2000,
                service_id = 0,
                service_type_id = 2,
                coupon = "",
                pick_shift = [2],
                items = dataModel.Select(detail => new Item
                {
                    name = detail.ProductVariant.Product.Name,
                    code = detail.ProductVariant.Barcode,
                    quantity = detail.Quantity,
                    price = (int)detail.PriceAtPurchase,
                    length = 12,
                    width = 12,
                    height = 12,
                    weight = 1200,
                    categorydto = new CategoryGHNDTO
                    {
                        level1 = "Áo"
                    }
                }).ToArray() // add data to ờ ray
            };

            return orderRequest; // Trả về OrderRequest
        }
        public async Task<CreateLogGHNDTO> CreateTrackingLogGHN(CreateLogGHNDTO data)
        {
            try
            {
                // check data
                if (data == null)
                {
                    throw new Exception("Dont have any data!");
                }

                var map = _mapper.Map<AuditLog>(data);
                var userCreate = await _ghNLogRepository.CreateAuditLog(map);
                var result = _mapper.Map<CreateLogGHNDTO>(userCreate);
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
