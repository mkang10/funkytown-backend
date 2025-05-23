using Domain.DTO.Response;
using Domain.DTOs;
using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Application.UseCases.GetWareHouseIdHandler;

namespace Application.UseCases
{

    public class GetWareHouseIdHandler
    {
        private readonly IWareHousesStockRepository _repository;

        public GetWareHouseIdHandler(IWareHousesStockRepository repository)
        {
            _repository = repository;
        }

        public async Task<Domain.DTOs.WarehouseStockDto?> GetByIdAsync(int id)
        {
            var entity = await _repository.GetByIdWithDetailsAsync(id);
            if (entity == null) return null;

            if (entity.Variant == null ||
                entity.Variant.Product == null ||
                entity.Variant.Color == null ||
                entity.Variant.Size == null ||
                entity.WareHouse == null)
            {
                throw new InvalidOperationException("Dữ liệu liên quan Variant hoặc Warehouse bị thiếu. Vui lòng kiểm tra database hoặc logic include.");
            }

            var dto = new Domain.DTOs.WarehouseStockDto
            {
                WareHouseStockId = entity.WareHouseStockId,
                VariantId = entity.VariantId,
                VariantName = $"{entity.Variant.Product.Name} - {entity.Variant.Color.ColorName} - {entity.Variant.Size.SizeName}",
                StockQuantity = entity.StockQuantity,
                WareHouseId = entity.WareHouseId,
                WareHouseName = entity.WareHouse.WarehouseName,
                AuditHistory = entity.WareHouseStockAudits?
                    .Select(a => new WarehouseStockAuditDto
                    {
                        AuditId = a.AuditId,
                        Action = a.Action,
                        QuantityChange = a.QuantityChange,
                        ActionDate = a.ActionDate,
                        ChangedBy = a.ChangedBy,
                        changedByName = a.ChangedByNavigation?.FullName ?? "Không rõ",
                        Note = a.Note
                    })
                    .OrderByDescending(a => a.ActionDate)
                    .ToList() ?? new List<WarehouseStockAuditDto>()
            };

            return dto;
        }


        public async Task<PaginatedResponseDTO<GetWareHouseStockRes>> GetByWarehouseIdAsync(
           int warehouseId,
           string? productNameFilter,
           string? sizeNameFilter,
           string? colorNameFilter,
           int? stockQuantityFilter,
           int page,
           int pageSize)
        {
            var allEntities = await _repository.GetByWarehouseIdAsync(warehouseId);

            var allDtos = allEntities.Select(entity => new GetWareHouseStockRes
            {
                WareHouseStockId = entity.WareHouseStockId,
                VariantId = entity.VariantId,
                ProductName = entity.Variant.Product.Name,
                VariantName = string.Join(" - ", new[]
                {
                    entity.Variant.Product.Name,
                    entity.Variant.Size?.SizeName,
                    entity.Variant.Color?.ColorName
                }.Where(s => !string.IsNullOrEmpty(s))),
                SizeName = entity.Variant.Size?.SizeName,
                ColorName = entity.Variant.Color?.ColorName,
                StockQuantity = entity.StockQuantity,
                WareHouseId = entity.WareHouseId,
                WareHouseName = entity.WareHouse.WarehouseName,

            });

            // Filters
            if (!string.IsNullOrEmpty(productNameFilter))
                allDtos = allDtos.Where(d => d.ProductName.ToLower().Contains(productNameFilter.ToLower()));
            if (!string.IsNullOrEmpty(sizeNameFilter))
                allDtos = allDtos.Where(d => d.SizeName != null && d.SizeName == sizeNameFilter);
            if (!string.IsNullOrEmpty(colorNameFilter))
                allDtos = allDtos.Where(d => d.ColorName != null && d.ColorName == colorNameFilter);
            if (stockQuantityFilter.HasValue)
                allDtos = allDtos.Where(d => d.StockQuantity == stockQuantityFilter.Value);

            var total = allDtos.Count();
            var paged = allDtos
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PaginatedResponseDTO<GetWareHouseStockRes>(paged, total, page, pageSize);
        }
    }
}
