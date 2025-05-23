using Domain.DTO.Request;
using Domain.DTO.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IPayOSService
    {
        Task<CreatePaymentResponse?> CreatePayment(int orderId, decimal amount, string paymentMethod, List<OrderItemRequest> orderItems);

     
    }
}
