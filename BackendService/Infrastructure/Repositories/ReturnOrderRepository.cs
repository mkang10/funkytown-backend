using Domain.Common_Model;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.HelperServices;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class ReturnOrderRepository : IReturnOrderRepository
    {
        private readonly FtownContext _context;

        public ReturnOrderRepository(FtownContext context)
        {
            _context = context;
        }

        public async Task CreateReturnOrderAsync(ReturnOrder returnOrder)
        {
            _context.ReturnOrders.Add(returnOrder);
            await _context.SaveChangesAsync();
        }


        public async Task<List<ReturnOrder>> GetReturnOrdersByAccountIdAsync(int accountId)
        {
            return await _context.ReturnOrders.Where(r => r.AccountId == accountId).ToListAsync();
        }
        public async Task AddReturnOrderItemsAsync(List<ReturnOrderItem> returnOrderItems)
        {
            _context.ReturnOrderItems.AddRange(returnOrderItems);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateReturnOrderStatusAsync(int returnOrderId, string status)
        {
            var returnOrder = await _context.ReturnOrders.FirstOrDefaultAsync(r => r.ReturnOrderId == returnOrderId);
            if (returnOrder != null)
            {
                returnOrder.Status = status;
                returnOrder.UpdatedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
        public async Task<PaginatedResult<ReturnOrder>> GetReturnOrdersAsync(
                                                        string? status,
                                                        string? returnOption,
                                                        DateTime? dateFrom,
                                                        DateTime? dateTo,
                                                        int? orderId,
                                                        int? returnOrderId,
                                                        int handledBy,
                                                        int pageNumber,
                                                        int pageSize)
        {
            IQueryable<ReturnOrder> query = _context.ReturnOrders
                .Include(ro => ro.Order).ThenInclude(o => o.OrderDetails)
                .Include(ro => ro.ReturnOrderItems);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(ro => ro.Status == status);

            if (!string.IsNullOrEmpty(returnOption))
                query = query.Where(ro => ro.ReturnOption == returnOption);

            if (dateFrom.HasValue)
                query = query.Where(ro => ro.CreatedDate >= dateFrom.Value);

            if (dateTo.HasValue)
                query = query.Where(ro => ro.CreatedDate <= dateTo.Value);

            if (orderId.HasValue)
                query = query.Where(ro => ro.OrderId == orderId.Value);

            if (returnOrderId.HasValue)
                query = query.Where(ro => ro.ReturnOrderId == returnOrderId.Value);

            //Chỉ lấy các đơn có người xử lý đúng với handledBy truyền vào
            query = query.Where(ro => ro.HandledBy == handledBy);

            query = query.OrderByDescending(ro => ro.ReturnOrderId);

            return await query.ToPaginatedResultAsync(pageNumber, pageSize);
        }


        public async Task<ReturnOrder?> GetByIdAsync(int returnOrderId)
        {
            return await _context.ReturnOrders
              .Include(ro => ro.Order)
              .Include(ro => ro.ReturnOrderItems)
              .FirstOrDefaultAsync(ro => ro.ReturnOrderId == returnOrderId);
        }

        public async Task UpdateAsync(ReturnOrder returnOrder)
        {
            _context.ReturnOrders.Update(returnOrder);
            await _context.SaveChangesAsync();
        }

        public async Task<List<ReturnOrder>> GetByStatusAsync(string status)
        {
            return await _context.ReturnOrders
              .Where(ro => ro.Status == status)
              .OrderByDescending(ro => ro.CreatedDate)
              .ToListAsync();
        }
    }
}
