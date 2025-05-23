using Domain.Common_Model;
using Domain.DTO.Response;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IOrderRepository
    {
        Task<(List<OrderAssignment> Items, int TotalRecords)>
        GetAllWithFilterAsync(
            OrderAssignmentFilterDto filter,
            int page,
            int pageSize);

        Task<OrderAssignment?> GetByOrderIdAsync(int orderId);
        Task SaveChangesAsync();

        Task<Order?> GetByIdAsync(int orderId);
        Task<OrderAssignment?> GetByIdWithDetailsAsync(int assignmentId);
        Task<Order> CreateOrderAsync(Order order);
        Task<Order?> GetOrderByIdAsync(int orderId);
        Task<List<Order>> GetOrderHistoryByAccountIdAsync(int accountId);
        Task UpdateOrderAsync(Order order);  
        Task<Order?> GetOrderWithDetailsAsync(int orderId); 
        Task SaveOrderDetailsAsync(List<OrderDetail> orderDetails);
        Task<PaginatedResult<Order>> GetOrdersByStatusPagedAsync(string? status, int? accountId, int pageNumber, int pageSize);
        Task<Order?> GetOrderByIdAsync(long orderId);
        Task<Order> GetOrderItemsWithOrderIdAsync(int orderId);
        Task<List<Order>> GetReturnableOrdersAsync(int accountId);
        Task UpdateOrderStatusAsync(int orderId, string newStatus);
        Task CreateAssignmentAsync(OrderAssignment assignment);
        Task<List<Order>> GetOrdersByShippingAddressId(int shippingAddressId);
        Task UpdateRangeAsync(IEnumerable<Order> orders);
        Task<List<Order>> GetCompletedOrdersAsync(DateTime? from, DateTime? to);
        Task<List<Order>> GetCompletedOrdersWithDetailsAsync(DateTime? from, DateTime? to);
        Task<Order?> GetOrderByIdGHNAsync(string orderId);
        Task UpdateOrderStatusGHNIdAsync(string orderId, string newStatus);
        Task<bool> IsOrderReturnableAsync(int orderId, int accountId);
        Task UpdateOrderStatusWithOrderAsync(Order order, string newStatus);

    }
}
