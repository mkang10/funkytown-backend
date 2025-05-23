using Domain.Common_Model;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IReturnOrderRepository
    {
        Task CreateReturnOrderAsync(ReturnOrder returnOrder);
        Task<List<ReturnOrder>> GetReturnOrdersByAccountIdAsync(int accountId);
        Task UpdateReturnOrderStatusAsync(int returnOrderId, string status);
        Task AddReturnOrderItemsAsync(List<ReturnOrderItem> returnOrderItems);
        Task<PaginatedResult<ReturnOrder>> GetReturnOrdersAsync(
                                            string? status,
                                            string? returnOption,
                                            DateTime? dateFrom,
                                            DateTime? dateTo,
                                            int? orderId,
                                            int? returnOrderId,
                                            int handledBy,
                                            int pageNumber,
                                            int pageSize);
        Task<ReturnOrder?> GetByIdAsync(int returnOrderId);
        Task UpdateAsync(ReturnOrder returnOrder);
        Task<List<ReturnOrder>> GetByStatusAsync(string status);
    }
}
