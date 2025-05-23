using AutoMapper;
using Domain.DTO.Response;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Application.UseCases.CompletedOrderHandler;
using static Domain.DTO.Response.OrderDoneRes;

namespace Application.UseCases
{
    
        public class CompletedOrderHandler 
        {
            private readonly IOrderRepository _orderRepo;
            private readonly IMapper _mapper;

            public CompletedOrderHandler(IOrderRepository orderRepo, IMapper mapper)
            {
                _orderRepo = orderRepo;
                _mapper = mapper;
            }

        public async Task<ResponseDTO<OrderResponseDTO>> CompleteOrderAsync(int orderId)
        {
            var order = await _orderRepo.GetByIdAsync(orderId);
            if (order == null)
            {
                return new ResponseDTO<OrderResponseDTO>(
                    data: null!,
                    status: false,
                    message: $"Không tìm thấy Order với ID = {orderId}"
                );
            }

            // Cập nhật trạng thái
            order.Status = "Confirmed";

            // Lưu vào DB
            await _orderRepo.SaveChangesAsync();

            // Gọi API GHN sau khi cập nhật thành công
            using var httpClient = new HttpClient();
            var apiUrl = $"https://ftorderserviceapi.azurewebsites.net/api/ghn/create-order/{orderId}";

            try
            {
                var response = await httpClient.PostAsync(apiUrl, null); // Nếu API không yêu cầu body
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                // Có thể log lỗi hoặc xử lý nếu API GHN thất bại, tuỳ bạn chọn có rollback hay không
                return new ResponseDTO<OrderResponseDTO>(
                    data: null!,
                    status: false,
                    message: $"Cập nhật thành công, nhưng lỗi khi tạo đơn hàng GHN: {ex.Message}"
                );
            }

            // Map về DTO
            var dto = _mapper.Map<OrderResponseDTO>(order);
            return new ResponseDTO<OrderResponseDTO>(
                data: dto,
                status: true,
                message: "Order đã được cập nhật thành Completed và đã gửi tạo đơn hàng GHN."
            );
        }


    }
}
