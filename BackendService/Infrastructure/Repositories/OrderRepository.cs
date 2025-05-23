using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.HelperServices.Models;
using Infrastructure.HelperServices;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Common_Model;
using Application.Enums;
using Domain.DTO.Response;

namespace Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly FtownContext _context;

        public OrderRepository(FtownContext context)
        {
            _context = context;
        }

        public Task<Order> CreateOrderAsync(Order order)
        {
            _context.Orders.Add(order);
            // KHÔNG gọi SaveChangesAsync ở đây
            return Task.FromResult(order);
        }


        public async Task<Order?> GetOrderByIdAsync(int orderId)
        {
            return await _context.Orders
                .Include(o => o.OrderDetails)
                .Include(o => o.Payments) // Lấy phương thức thanh toán
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
        }

        public async Task<Order?> GetOrderByIdAsync(long orderId)
        {
            return await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId);
        }
        public async Task<List<Order>> GetOrderHistoryByAccountIdAsync(int accountId)
        {
            return await _context.Orders
                .Where(o => o.AccountId == accountId)
                .OrderByDescending(o => o.CreatedDate)
                .ToListAsync();
        }

        public async Task UpdateOrderAsync(Order order)
        {
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
        }

        public async Task<Order?> GetOrderWithDetailsAsync(int orderId)
        {
            return await _context.Orders
                .Include(o => o.OrderDetails)
                .Include(o => o.Payments) // ✅ Lấy thêm Payment
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
        }

        public async Task SaveOrderDetailsAsync(List<OrderDetail> orderDetails)
        {
            _context.OrderDetails.AddRange(orderDetails);
            await _context.SaveChangesAsync();
        }
        public async Task<PaginatedResult<Order>> GetOrdersByStatusPagedAsync(string? status, int? accountId, int pageNumber, int pageSize)
        {
            var query = _context.Orders
                .Include(o => o.OrderDetails)
                .Include(o => o.Payments)
                .OrderByDescending(o => o.CreatedDate)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(o => o.Status == status);
            }

            if (accountId.HasValue)
            {
                query = query.Where(o => o.AccountId == accountId);
            }

            return await query.ToPaginatedResultAsync(pageNumber, pageSize); // ✅ Reuse!
        }

        public async Task<List<Order>> GetReturnableOrdersAsync(int accountId)
        {
            var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);

            
            var completed = OrderStatus.Completed.ToString().ToLower(); // "completed"

            var orders = await _context.Orders
                .AsNoTracking()
                .Where(o =>
                    o.AccountId == accountId &&
                    o.Status.ToLower() == completed &&       // không phân biệt hoa thường
                    o.CompletedDate.HasValue &&
                    o.CompletedDate.Value >= sevenDaysAgo
                )
                .Include(o => o.OrderDetails)
                .OrderByDescending(o => o.CreatedDate)
                .ToListAsync();

            return orders;
        }


        public async Task UpdateOrderStatusAsync(int orderId, string newStatus)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return;

            order.Status = newStatus;
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateOrderStatusWithOrderAsync(Order order, string newStatus)
        {
            order.Status = newStatus;
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
        }
        public async Task<Order> GetOrderItemsWithOrderIdAsync(int orderId)
        {
            return await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.ProductVariant)
                .ThenInclude(pv => pv.Product)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.ProductVariant)
                .ThenInclude(pv => pv.Size)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.ProductVariant)
                .ThenInclude(pv => pv.Color)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
        }
        public async Task CreateAssignmentAsync(OrderAssignment assignment)
        {
            _context.OrderAssignments.Add(assignment);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Order>> GetOrdersByShippingAddressId(int shippingAddressId)
        {
            return await _context.Orders
                .Where(o => o.ShippingAddressId == shippingAddressId)
                .ToListAsync();
        }

        public async Task UpdateRangeAsync(IEnumerable<Order> orders)
        {
            _context.Orders.UpdateRange(orders);
            await _context.SaveChangesAsync();
        }
        public async Task<List<Order>> GetCompletedOrdersAsync(DateTime? from, DateTime? to)
        {
            var query = _context.Orders
                .Where(o => o.Status == "completed" && o.CreatedDate.HasValue)
                .Include(o => o.OrderDetails)
                .AsQueryable();

            if (from.HasValue)
            {
                var fromDate = from.Value.Date;
                query = query.Where(o => o.CreatedDate.HasValue && o.CreatedDate.Value.Date >= fromDate);
            }

            if (to.HasValue)
            {
                var toDate = to.Value.Date;
                query = query.Where(o => o.CreatedDate.HasValue && o.CreatedDate.Value.Date <= toDate);
            }

            return await query.ToListAsync();
        }
        public async Task<List<Order>> GetCompletedOrdersWithDetailsAsync(DateTime? from, DateTime? to)
        {
            var query = _context.Orders
                .Where(o => o.Status == "completed" && o.CreatedDate.HasValue)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.ProductVariant)
                        .ThenInclude(pv => pv.Product)
                            .ThenInclude(p => p.Category)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.ProductVariant)
                        .ThenInclude(pv => pv.Size)      // ✅ THÊM dòng này
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.ProductVariant)
                        .ThenInclude(pv => pv.Color)     // ✅ THÊM dòng này
                .AsQueryable();

            if (from.HasValue)
                query = query.Where(o => o.CreatedDate.Value.Date >= from.Value.Date);

            if (to.HasValue)
                query = query.Where(o => o.CreatedDate.Value.Date <= to.Value.Date);

            return await query.ToListAsync();
        }


        public async Task UpdateOrderStatusGHNIdAsync(string orderId, string newStatus)
        {
            var order = await _context.Orders.SingleOrDefaultAsync(o => o.Ghnid == orderId);
            if (order == null) return;

            order.Status = newStatus;
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
        }

        public Task<Order?> GetOrderByIdGHNAsync(string orderId)
        {
            var data = _context.Orders.SingleOrDefaultAsync(o => o.Ghnid == orderId);
            return data;
        }

        public async Task<bool> IsOrderReturnableAsync(int orderId, int accountId)
        {
            var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);

            return await _context.Orders.AnyAsync(o =>
                o.OrderId == orderId &&
                o.AccountId == accountId &&
                o.Status == "completed" &&
                o.CompletedDate != null &&
                o.CompletedDate >= sevenDaysAgo);
        }

        public async Task<(List<OrderAssignment>, int)> GetAllWithFilterAsync(
    OrderAssignmentFilterDto f,
    int page,
    int pageSize)
        {
            var q = _context.OrderAssignments
                // Include Order → OrderDetails → ProductVariant → Product
                .Include(oa => oa.Order)
                    .ThenInclude(o => o.OrderDetails)
                        .ThenInclude(od => od.ProductVariant)
                            .ThenInclude(pv => pv.Product)
                // Include ProductVariant → Size
                .Include(oa => oa.Order)
                    .ThenInclude(o => o.OrderDetails)
                        .ThenInclude(od => od.ProductVariant)
                            .ThenInclude(pv => pv.Size)
                // Include ProductVariant → Color
                .Include(oa => oa.Order)
                    .ThenInclude(o => o.OrderDetails)
                        .ThenInclude(od => od.ProductVariant)
                            .ThenInclude(pv => pv.Color)
                .AsNoTracking()
                .AsQueryable();

            // 1. Filter OrderAssignment
            if (f.AssignmentId.HasValue)
                q = q.Where(oa => oa.AssignmentId == f.AssignmentId);
            if (f.ShopManagerId.HasValue)
                q = q.Where(oa => oa.ShopManagerId == f.ShopManagerId);
            if (f.StaffId.HasValue)
                q = q.Where(oa => oa.StaffId == f.StaffId);
            if (f.AssignmentDateFrom.HasValue)
                q = q.Where(oa => oa.AssignmentDate >= f.AssignmentDateFrom);
            if (f.AssignmentDateTo.HasValue)
                q = q.Where(oa => oa.AssignmentDate <= f.AssignmentDateTo);
            if (!string.IsNullOrWhiteSpace(f.CommentsContains))
                q = q.Where(oa => oa.Comments.Contains(f.CommentsContains));

            // 2. Filter Order
            if (f.OrderCreatedDateFrom.HasValue)
                q = q.Where(oa => oa.Order.CreatedDate >= f.OrderCreatedDateFrom);
            if (f.OrderCreatedDateTo.HasValue)
                q = q.Where(oa => oa.Order.CreatedDate <= f.OrderCreatedDateTo);
            if (!string.IsNullOrWhiteSpace(f.OrderStatus))
                q = q.Where(oa => oa.Order.Status == f.OrderStatus);
            if (f.MinOrderTotal.HasValue)
                q = q.Where(oa => oa.Order.OrderTotal >= f.MinOrderTotal);
            if (f.MaxOrderTotal.HasValue)
                q = q.Where(oa => oa.Order.OrderTotal <= f.MaxOrderTotal);
            if (!string.IsNullOrWhiteSpace(f.FullNameContains))
                q = q.Where(oa => oa.Order.FullName.Contains(f.FullNameContains));

            // 3. Count & Paging
            var total = await q.CountAsync();
            var data = await q
    .OrderByDescending(oa => oa.AssignmentId)   // sort giảm dần
    .Skip((page - 1) * pageSize)
    .Take(pageSize)
    .ToListAsync();

            return (data, total);
        }

        public async Task<OrderAssignment?> GetByOrderIdAsync(int orderId)
        {
            return await _context.OrderAssignments
                .Include(oa => oa.Order)
                .FirstOrDefaultAsync(oa => oa.OrderId == orderId);
        }

        public async Task<Order?> GetByIdAsync(int orderId)
        {
            return await _context.Orders
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
        }


        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<OrderAssignment?> GetByIdWithDetailsAsync(int assignmentId)
        {
            return await _context.OrderAssignments
                .Include(oa => oa.Order)
                    .ThenInclude(o => o.OrderDetails)
                        .ThenInclude(od => od.ProductVariant)
                            .ThenInclude(pv => pv.Product)
                .Include(oa => oa.Order)
                    .ThenInclude(o => o.OrderDetails)
                        .ThenInclude(od => od.ProductVariant)
                            .ThenInclude(pv => pv.Size)
                .Include(oa => oa.Order)
                    .ThenInclude(o => o.OrderDetails)
                        .ThenInclude(od => od.ProductVariant)
                            .ThenInclude(pv => pv.Color)
                .AsNoTracking()
                .FirstOrDefaultAsync(oa => oa.AssignmentId == assignmentId);
        }
    }
}
 
