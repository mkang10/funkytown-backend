using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IPaymentRepository
    {
        Task SavePaymentAsync(Payment payment);
        Task<Payment?> GetPaymentByOrderIdAsync(int orderId);
        Task UpdatePaymentAsync(Payment payment);
        Task<Payment?> GetPaymentByOrderIdAsync(long orderId);
        Task<string?> GetPaymentMethodByOrderIdAsync(int orderId);
        Task<Payment?> GetPaymentByOrderCodeAsync(long orderCode);
    }
}
