using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IGHNLogRepository
    {
        public Task<AuditLog> CreateAuditLog(AuditLog data);
        public Task<List<OrderDetail>> GetDataOrder(int id);

        public Task<Order> AddGHNIdtoOrderTable(Order data);
        public Task<Order> GetOrderById(int id);


    }
}
