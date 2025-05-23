using Application.Interfaces;
using Domain.Common_Model;
using Domain.Commons;
using Domain.DTO.Response;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class RevenueHandler
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<RevenueHandler> _logger;
        private readonly IPaginationHelper _paginationHelper;
        public RevenueHandler(IOrderRepository orderRepository, ILogger<RevenueHandler> logger, IPaginationHelper paginationHelper)
        {
            _orderRepository = orderRepository;
            _logger = logger;
            _paginationHelper = paginationHelper;
        }

        public async Task<ResponseDTO<RevenueSummaryResponse>> GetRevenueSummaryAsync(DateTime? from, DateTime? to)
        {
            var orders = await _orderRepository.GetCompletedOrdersAsync(from, to);

            var totalRevenue = orders
                .SelectMany(o => o.OrderDetails)
                .Sum(od => od.PriceAtPurchase * od.Quantity);

            var totalOrders = orders.Count;

            var totalProductsSold = orders
                .SelectMany(o => o.OrderDetails)
                .Sum(od => od.Quantity);

            var response = new RevenueSummaryResponse
            {
                TotalRevenue = totalRevenue,
                TotalOrders = totalOrders,
                TotalProductsSold = totalProductsSold
            };

            return new ResponseDTO<RevenueSummaryResponse>(response, true, "Lấy thống kê doanh thu thành công");
        }
        public async Task<ResponseDTO<PaginatedResult<OrderSummaryItem>>> GetRevenueOrdersAsync(
                                                                                DateTime? from, DateTime? to, int pageNumber = 1, int pageSize = 10)
        {
            _logger.LogInformation("Getting completed orders from {From} to {To}", from, to);

            var orders = await _orderRepository.GetCompletedOrdersAsync(from, to);

            _logger.LogInformation("Total completed orders fetched: {Count}", orders.Count);

            foreach (var order in orders)
            {
                _logger.LogDebug("OrderId: {OrderId}, CreatedDate: {CreatedDate}",
                    order.OrderId, order.CreatedDate?.ToString("yyyy-MM-dd HH:mm:ss"));
            }

            var result = orders.Select(o => new OrderSummaryItem
            {
                OrderId = o.OrderId,
                CreatedDate = o.CreatedDate,
                CustomerName = o.FullName,
                TotalQuantity = o.OrderDetails.Sum(od => od.Quantity),
                TotalPrice = o.OrderDetails.Sum(od => od.PriceAtPurchase * od.Quantity)
            });

            var paginated = _paginationHelper.PaginateInMemory(result, pageNumber, pageSize);

            return new ResponseDTO<PaginatedResult<OrderSummaryItem>>(paginated, true, "Lấy danh sách đơn hàng doanh thu thành công");

        }

        public async Task<ResponseDTO<List<TopSellingProductResponse>>> GetTopSellingProductsAsync(DateTime? from, DateTime? to, int top = 10)
        {
            var orders = await _orderRepository.GetCompletedOrdersWithDetailsAsync(from, to);

            var productStats = orders
                .SelectMany(o => o.OrderDetails)
                .GroupBy(od => new
                {
                    od.ProductVariant.Product.ProductId,
                    od.ProductVariant.Product.Name,
                    od.ProductVariant.Product.ImagePath
                })
                .Select(g => new TopSellingProductResponse
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.Name,
                    ImagePath = g.Key.ImagePath,
                    QuantitySold = g.Sum(x => x.Quantity),
                    Revenue = g.Sum(x => x.Quantity * x.PriceAtPurchase)
                })
                .OrderByDescending(x => x.QuantitySold)
                .Take(top)
                .ToList();

            return new ResponseDTO<List<TopSellingProductResponse>>(productStats, true, "Lấy top sản phẩm bán chạy thành công");
        }

        public async Task<ResponseDTO<PaginatedResult<RevenueByDateResponse>>> GetRevenueByDateAsync(
                                                                                                        DateTime from, DateTime to,
                                                                                                        string groupBy = "day",
                                                                                                        int pageNumber = 1, int pageSize = 10)
        {
            var orders = await _orderRepository.GetCompletedOrdersWithDetailsAsync(from, to);

            var orderDetails = orders.SelectMany(o => o.OrderDetails)
                .Select(od => new
                {
                    Date = od.Order.CreatedDate!.Value,
                    Revenue = od.Quantity * od.PriceAtPurchase
                });

            var grouped = groupBy.ToLower() switch
            {
                "day" => orderDetails.GroupBy(x => x.Date.ToString("yyyy-MM-dd")),
                "month" => orderDetails.GroupBy(x => x.Date.ToString("yyyy-MM")),
                "year" => orderDetails.GroupBy(x => x.Date.ToString("yyyy")),
                _ => throw new ArgumentException("Invalid groupBy. Use: day, month, or year.")
            };

            var result = grouped.Select(g => new RevenueByDateResponse
            {
                TimePeriod = g.Key,
                TotalRevenue = g.Sum(x => x.Revenue)
            })
            .OrderBy(x => x.TimePeriod)
            .ToList();

            var paginated = _paginationHelper.PaginateInMemory(result, pageNumber, pageSize);

            return new ResponseDTO<PaginatedResult<RevenueByDateResponse>>(paginated, true, "Lấy thống kê doanh thu theo thời gian thành công");
        }

        public async Task<ResponseDTO<PaginatedResult<VariantRevenueItem>>> GetRevenueByProductAsync(
                                                                                        int productId, DateTime? from, DateTime? to,
                                                                                        int pageNumber = 1, int pageSize = 10)
        {
            var orders = await _orderRepository.GetCompletedOrdersWithDetailsAsync(from, to);

            var allDetails = orders
                .SelectMany(o => o.OrderDetails)
                .Where(od => od.ProductVariant.Product.ProductId == productId)
                .ToList();

            if (!allDetails.Any())
            {
                return new ResponseDTO<PaginatedResult<VariantRevenueItem>>(null, true, "Không có dữ liệu doanh thu cho sản phẩm này");
            }

            var variants = allDetails
                .GroupBy(x => x.ProductVariant.VariantId)
                .Select(g => new VariantRevenueItem
                {
                    VariantId = g.Key,
                    Size = g.First().ProductVariant.Size?.SizeName,
                    Color = g.First().ProductVariant.Color?.ColorName,
                    QuantitySold = g.Sum(x => x.Quantity),
                    Revenue = g.Sum(x => x.Quantity * x.PriceAtPurchase)
                })
                .ToList();

            var paginatedVariants = _paginationHelper.PaginateInMemory(variants, pageNumber, pageSize);

            return new ResponseDTO<PaginatedResult<VariantRevenueItem>>(paginatedVariants, true, "Lấy chi tiết doanh thu sản phẩm thành công");
        }


    }

}
