using AutoMapper;
using Domain.DTO.Response;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{

    public class GetOrderHandler
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;
        public GetOrderHandler(IOrderRepository orderRepository, IMapper mapper)
        {
            _orderRepository = orderRepository;
            _mapper = mapper;
        }

        public async Task<PaginatedResponseDTO<OrderAssignmentDto>> GetAllAsync(
    OrderAssignmentFilterDto filter,
    int page,
    int pageSize)
        {
            var (entities, total) = await _orderRepository
                .GetAllWithFilterAsync(filter, page, pageSize);

            var dtos = entities.Select(oa => new OrderAssignmentDto
            {
                AssignmentId = oa.AssignmentId,
                ShopManagerId = oa.ShopManagerId,
                StaffId = oa.StaffId,
                AssignmentDate = oa.AssignmentDate,
                Comments = oa.Comments,
                Order = new OrderDto
                {
                    OrderId = oa.Order.OrderId,
                    CreatedDate = oa.Order.CreatedDate,
                    Status = oa.Order.Status,
                    OrderTotal = oa.Order.OrderTotal,
                    ShippingCost = oa.Order.ShippingCost,
                    FullName = oa.Order.FullName,
                    Email = oa.Order.Email,
                    PhoneNumber = oa.Order.PhoneNumber,
                    Address = oa.Order.Address,
                    City = oa.Order.City,
                    District = oa.Order.District,
                    Country = oa.Order.Country,
                    Province = oa.Order.Province,
                    OrderDetails = oa.Order.OrderDetails.Select(od => new OrderDetailDto
                    {
                        OrderDetailId = od.OrderDetailId,
                        ProductVariantId = od.ProductVariantId,
                        ProductName = od.ProductVariant.Product.Name,
                        SizeName = od.ProductVariant.Size?.SizeName,
                        ColorName = od.ProductVariant.Color?.ColorName,
                        Quantity = od.Quantity,
                        PriceAtPurchase = od.PriceAtPurchase,
                        DiscountApplied = od.DiscountApplied
                    }).ToList()
                }
            }).ToList();

            return new PaginatedResponseDTO<OrderAssignmentDto>(
                dtos, total, page, pageSize);
        }
        public async Task<OrderAssignmentDto?> GetByIdAsync(int assignmentId)
        {
            var oa = await _orderRepository.GetByIdWithDetailsAsync(assignmentId);
            if (oa == null) return null;

            return new OrderAssignmentDto
            {
                AssignmentId = oa.AssignmentId,
                ShopManagerId = oa.ShopManagerId,
                StaffId = oa.StaffId,
                AssignmentDate = oa.AssignmentDate,
                Comments = oa.Comments,
                Order = new OrderDto
                {
                    OrderId = oa.Order.OrderId,
                    CreatedDate = oa.Order.CreatedDate,
                    Status = oa.Order.Status,
                    OrderTotal = oa.Order.OrderTotal,
                    ShippingCost = oa.Order.ShippingCost,
                    FullName = oa.Order.FullName,
                    Email = oa.Order.Email,
                    PhoneNumber = oa.Order.PhoneNumber,
                    Address = oa.Order.Address,
                    City = oa.Order.City,
                    District = oa.Order.District,
                    Country = oa.Order.Country,
                    Province = oa.Order.Province,
                    OrderDetails = oa.Order.OrderDetails.Select(od => new OrderDetailDto
                    {
                        OrderDetailId = od.OrderDetailId,
                        ProductVariantId = od.ProductVariantId,
                        ProductName = od.ProductVariant.Product.Name,
                        SizeName = od.ProductVariant.Size?.SizeName,
                        ColorName = od.ProductVariant.Color?.ColorName,
                        Quantity = od.Quantity,
                        PriceAtPurchase = od.PriceAtPurchase,
                        DiscountApplied = od.DiscountApplied
                    }).ToList()
                }
            };
        }
    }
}