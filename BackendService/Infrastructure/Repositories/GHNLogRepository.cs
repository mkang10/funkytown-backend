using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repository
{
    public class GHNLogRepository : IGHNLogRepository
    {
        private readonly FtownContext _context;

        public GHNLogRepository(FtownContext context)
        {
            _context = context;
        }

        public async Task<Order> AddGHNIdtoOrderTable(Order data)
        {
            _context.Update(data);
            await _context.SaveChangesAsync();
            return data;
        }

        public async Task<AuditLog> CreateAuditLog(AuditLog data)
        {
            _context.Add(data);
            await _context.SaveChangesAsync();
            return data;
        }

        public async Task<List<OrderDetail>> GetDataOrder(int id)
        {
            try
            {
                if (id == 0)
                {
                    throw new ArgumentNullException("No id");
                }

                var data = await _context.OrderDetails
                    .Include(o => o.ProductVariant).ThenInclude(od => od.Product)
                    .Include(o => o.ProductVariant).ThenInclude(od => od.Size)
                    .Include(o => o.Order).ThenInclude(od => od.WareHouse)
                    .Include(o => o.Order).ThenInclude(od => od.ShippingAddress)
                    .Where(a => a.OrderId == id)
                    .ToListAsync(); // Sử dụng ToListAsync để lấy danh sách

                return data;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        public async Task<Order> GetOrderById(int id)
        {
            var data = await _context.Orders.SingleOrDefaultAsync(x => x.OrderId.Equals(id));
            return data;
        }


    }
}
