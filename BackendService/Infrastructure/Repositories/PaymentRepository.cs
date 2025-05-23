using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly FtownContext _context;

        public PaymentRepository(FtownContext context)
        {
            _context = context;
        }

        public async Task SavePaymentAsync(Payment payment)
        {
            _context.Payments.Add(payment);
        }

        public async Task<Payment?> GetPaymentByOrderIdAsync(int orderId)
        {
            return await _context.Payments
                .FirstOrDefaultAsync(p => p.OrderId == orderId);
        }
        public async Task<Payment?> GetPaymentByOrderIdAsync(long orderId)
        {
            return await _context.Payments.FirstOrDefaultAsync(p => p.OrderId == orderId);
        }
        public async Task UpdatePaymentAsync(Payment payment)
        {
            _context.Payments.Update(payment);
            await _context.SaveChangesAsync();
        }
        public async Task<string?> GetPaymentMethodByOrderIdAsync(int orderId)
        {
            return await _context.Payments
                .Where(p => p.OrderId == orderId)
                .Select(p => p.PaymentMethod)
                .FirstOrDefaultAsync();
        }
        public async Task<Payment?> GetPaymentByOrderCodeAsync(long orderCode)
        {
            return await _context.Payments
                .FirstOrDefaultAsync(p => p.OrderCode == orderCode);
        }
    }
}
