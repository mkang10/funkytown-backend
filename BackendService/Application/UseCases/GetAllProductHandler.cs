using AutoMapper;
using Domain.DTO.Response;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Application.UseCases.GetAllProductHandler;

namespace Application.UseCases
{
    
        public class GetAllProductHandler 
        {
            private readonly IProductRepository _repo;
        private readonly IMapper _mapper;

        public GetAllProductHandler(IProductRepository repo, IMapper mapper)
            {
                _repo = repo;
            _mapper = mapper;

        }

        public async Task<PaginatedResponseDTO<ProductDto>>
GetAllProductsAsync(
    string? nameFilter,
    string? descriptionFilter,
    int? categoryFilter,
    string? originFilter,
    string? modelFilter,
    string? occasionFilter,
    string? styleFilter,
    string? materialFilter,
    string? statusFilter,
    string? skuFilter,    // ← Tham số mới
    int page,
    int pageSize)
        {
            // 1. Load toàn bộ Product kèm Category, Images, Variants
            var all = await _repo.GetAllAsync(); // trả về IEnumerable<Product> đã Include các nav-props

            // 2. Chuyển sang IQueryable để dễ chaining filter
            var query = all.AsQueryable();

            // 3. Áp dụng các filter cơ bản
            if (!string.IsNullOrEmpty(nameFilter))
                query = query.Where(p =>
                    p.Name.Contains(nameFilter, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(descriptionFilter))
                query = query.Where(p =>
                    p.Description != null &&
                    p.Description.Contains(descriptionFilter, StringComparison.OrdinalIgnoreCase));

            if (categoryFilter.HasValue)
                query = query.Where(p => p.CategoryId == categoryFilter.Value);

            if (!string.IsNullOrEmpty(originFilter))
                query = query.Where(p =>
                    p.Origin != null &&
                    p.Origin.Contains(originFilter, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(modelFilter))
                query = query.Where(p =>
                    p.Model != null &&
                    p.Model.Contains(modelFilter, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(occasionFilter))
                query = query.Where(p =>
                    p.Occasion != null &&
                    p.Occasion.Contains(occasionFilter, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(styleFilter))
                query = query.Where(p =>
                    p.Style != null &&
                    p.Style.Contains(styleFilter, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(materialFilter))
                query = query.Where(p =>
                    p.Material != null &&
                    p.Material.Contains(materialFilter, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(statusFilter))
                query = query.Where(p =>
                    p.Status != null &&
                    p.Status.Contains(statusFilter, StringComparison.OrdinalIgnoreCase));

            // 4. Áp dụng filter theo SKU của ProductVariant
            if (!string.IsNullOrEmpty(skuFilter))
            {
                query = query.Where(p =>
                    p.ProductVariants.Any(v =>
                        v.Sku != null &&
                        v.Sku.Contains(skuFilter, StringComparison.OrdinalIgnoreCase)));
            }

            // 5. Đếm tổng số phần tử sau filter
            var total = query.Count();

            // 6. Phân trang và project sang DTO
            var items = query
                .OrderBy(p => p.ProductId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProductDto
                {
                    ProductId = p.ProductId,
                    Name = p.Name,
                    Description = p.Description,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name,
                    ImagePath = p.ProductImages.FirstOrDefault(pi => pi.IsMain).ImagePath,
                    Origin = p.Origin,
                    Model = p.Model,
                    Occasion = p.Occasion,
                    Style = p.Style,
                    Material = p.Material,
                    Status = p.Status
                })
                .ToList();

            // 7. Trả về kết quả phân trang
            return new PaginatedResponseDTO<ProductDto>(items, total, page, pageSize);
        }
        public async Task<PaginatedResponseDTO<ProductVariantResponseDto>> GetAllProductVariantsAsync(int page, int pageSize, string? search = null)
        {
            var pagedResult = await _repo.GetAllAsync(page, pageSize, search);
            var dtos = _mapper.Map<List<ProductVariantResponseDto>>(pagedResult.Data);
            return new PaginatedResponseDTO<ProductVariantResponseDto>(dtos, pagedResult.TotalRecords, pagedResult.Page, pagedResult.PageSize);
        }
    }
}
