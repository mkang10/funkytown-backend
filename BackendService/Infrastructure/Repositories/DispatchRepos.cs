using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.DTO.Request;
using Domain.DTO.Response;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Domain.DTO.Request.StoreExportStoreDetailReq;

namespace Infrastructure
{
    public class DispatchRepos : IDispatchRepos { 
    
        
    private readonly FtownContext _context;
        private readonly IMapper _mapper;

        public DispatchRepos(IMapper mapper,FtownContext context)
        {
            _context = context;
            _mapper = mapper;

        }

        public void Add(Dispatch dispatch)
        {
            _context.Dispatches.Add(dispatch);
        }

        public Task SaveChangesAsync()
        {
            return _context.SaveChangesAsync();
        }

        public async Task<int> GetApprovedOutboundQuantityAsync(int warehouseId, int variantId)
        {
            // Dùng DispatchStoreDetails vì WarehouseId nằm ở đây
            var query = from dsd in _context.StoreExportStoreDetails
                        join dd in _context.DispatchDetails on dsd.DispatchDetailId equals dd.DispatchDetailId
                        join dj in _context.Dispatches on dd.DispatchId equals dj.DispatchId
                        where dsd.WarehouseId == warehouseId
                              && dd.VariantId == variantId
                              && dj.Status == "Approved"
                        select dsd.AllocatedQuantity;

            return await query.SumAsync(q => (int?)q) ?? 0;
        }
        public async Task<Dispatch?> GetDispatchByTransferIdAsync(int transferId)
        {
            var transfer = await _context.Transfers
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TransferOrderId == transferId);
            if (transfer == null || transfer.DispatchId == 0)
                return null;

            return await _context.Dispatches
                .Include(d => d.DispatchDetails)
                    .ThenInclude(dd => dd.Variant)
                        .ThenInclude(v => v.Product)
                .Include(d => d.DispatchDetails)
                    .ThenInclude(dd => dd.Variant)
                        .ThenInclude(v => v.Color)
                .Include(d => d.DispatchDetails)
                    .ThenInclude(dd => dd.Variant)
                        .ThenInclude(v => v.Size)
                .Include(d => d.DispatchDetails)
                    .ThenInclude(dd => dd.StoreExportStoreDetails)
                .FirstOrDefaultAsync(d => d.DispatchId == transfer.DispatchId);
        }
        // =============Duc Anh 12-04-2025 22:34================
        public async Task<Dispatch> GetJSONDispatchById(int id) // Truyen DispatchId vo
        {
            var data = await _context.Dispatches.
                Include(o => o.CreatedByNavigation).
                Include(o => o.DispatchDetails)
                        .ThenInclude(od => od.StoreExportStoreDetails)
                        .ThenInclude(od => od.HandleByNavigation).ThenInclude(od => od.Account).

                Include(o => o.DispatchDetails)
                        .ThenInclude(od => od.Variant.Product).
                Include(o => o.DispatchDetails)
                        .ThenInclude(od => od.StoreExportStoreDetails)
                        .ThenInclude(od => od.Warehouse)
                .Include(d => d.DispatchDetails)
                    .ThenInclude(dd => dd.Variant)
                        .ThenInclude(v => v.Color)
                .Include(d => d.DispatchDetails)
                    .ThenInclude(dd => dd.Variant)
                        .ThenInclude(v => v.Size).
                Include(o => o.DispatchDetails)
                        .ThenInclude(od => od.StoreExportStoreDetails)
                        .ThenInclude(od => od.StaffDetail).ThenInclude(od => od.Account).
                FirstOrDefaultAsync(x => x.DispatchId == id);
            return data;
        }

        public async Task ReloadAsync(Dispatch dispatch)
        {
            await _context.Entry(dispatch).ReloadAsync();
        }
        public async Task AddAsync(Dispatch dispatch)
           => await _context.Dispatches.AddAsync(dispatch);
       
        public async Task<StoreExportStoreDetail> GetStoreExportStoreDetailById(int importId)
        {
            var data = await _context.StoreExportStoreDetails
                           .Include(od => od.Warehouse)
                           .Include(od => od.StaffDetail).ThenInclude(oc => oc.Account)
                           .Include(od => od.HandleByNavigation.Account)
                           .Include(od => od.DispatchDetail)
                                   .ThenInclude(c => c.Dispatch).
                            Include(o => o.DispatchDetail)
                                    .ThenInclude(od => od.Variant.Product)
                            .Include(o => o.DispatchDetail)
                                    .ThenInclude(od => od.Variant.Size)
                                     .Include(o => o.DispatchDetail)
                                    .ThenInclude(od => od.Variant.Color)
                           .FirstOrDefaultAsync(o => o.DispatchStoreDetailId == importId);
            return data;
        }
        public async Task<Dispatch?> GetByIdAssignAsync(int dispatchId)
        {
            return await _context.Dispatches
                .Include(d => d.DispatchDetails)
                    .ThenInclude(dd => dd.StoreExportStoreDetails)
                .FirstOrDefaultAsync(d => d.DispatchId == dispatchId);
        }

        // Lấy Dispatch theo Id (không include các navigation property nếu không cần thiết)
        public async Task<Dispatch?> GetByIdAsync(int dispatchId)
        {
            return await _context.Dispatches.FindAsync(dispatchId);
        }

        // Lấy danh sách Dispatch có cùng OriginalDispatchId (giả sử bạn có thuộc tính này trong Dispatch)
        public async Task<List<Dispatch>> GetAllByOriginalDispatchIdAsync(int originalDispatchId)
        {
            return await _context.Dispatches
                .Where(d => d.OriginalId == originalDispatchId)
                .ToListAsync();
        }

       
        public async Task<PaginatedResponseDTO<DispatchResponseDto>> GetAllDispatchAsync(int page, int pageSize, DispatchFilterDto filter)
        {
            var query = _context.Dispatches
                .Include(d => d.CreatedByNavigation)
                .Include(d => d.DispatchDetails)
                    .ThenInclude(dd => dd.Variant)
                        .ThenInclude(v => v.Product)
                .Include(d => d.DispatchDetails)
                    .ThenInclude(dd => dd.StoreExportStoreDetails)
                        .ThenInclude(s => s.Warehouse)
                .Include(d => d.DispatchDetails)
                    .ThenInclude(dd => dd.StoreExportStoreDetails)
                        .ThenInclude(s => s.StaffDetail)
                            .ThenInclude(sd => sd.Account)
                .AsQueryable();

            // Filter
            if (!string.IsNullOrEmpty(filter.Status))
                query = query.Where(d => d.Status == filter.Status);

            if (!string.IsNullOrEmpty(filter.ReferenceNumber))
                query = query.Where(d => d.ReferenceNumber!.Contains(filter.ReferenceNumber));

            if (filter.FromDate.HasValue)
                query = query.Where(d => d.CreatedDate >= filter.FromDate.Value);

            if (filter.ToDate.HasValue)
                query = query.Where(d => d.CreatedDate <= filter.ToDate.Value);

            if (filter.WarehouseId.HasValue)
                query = query.Where(d => d.DispatchDetails.Any(dd =>
                    dd.StoreExportStoreDetails.Any(sd => sd.WarehouseId == filter.WarehouseId)));

            if (filter.StaffDetailId.HasValue)
                query = query.Where(d => d.DispatchDetails.Any(dd =>
                    dd.StoreExportStoreDetails.Any(sd => sd.StaffDetailId == filter.StaffDetailId)));

            var total = await query.CountAsync();

            var data = await query
                .OrderByDescending(d => d.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ProjectTo<DispatchResponseDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return new PaginatedResponseDTO<DispatchResponseDto>(data, total, page, pageSize);
        }

        public async Task<Dispatch?> GetByIdDispatchAssignAsync(int dispatchId)
        {
            return await _context.Dispatches
                .Include(i => i.DispatchDetails)
                    .ThenInclude(detail => detail.StoreExportStoreDetails)
                .FirstOrDefaultAsync(i => i.DispatchId == dispatchId);
        }
        public async Task UpdateAsync(Dispatch dispatch)
        {
            _context.Dispatches.Update(dispatch);
            await _context.SaveChangesAsync();
        }

        public async Task<PaginatedResponseDTO<ExportDetailDto>> GetAllExportStoreDetailsAsync(
    int page,
    int pageSize,
    StoreExportStoreDetailFilterDto filter)
        {
            var query = _context.StoreExportStoreDetails
                .AsNoTracking()
                .Include(e => e.Warehouse)
                .Include(e => e.StaffDetail)
                    .ThenInclude(sd => sd.Account)
                .Include(e => e.DispatchDetail) // Đảm bảo include DispatchDetail
                .AsQueryable();

            // --- Filters ---
            if (filter.DispatchDetailId.HasValue)
                query = query.Where(e => e.DispatchDetailId == filter.DispatchDetailId.Value);

            if (filter.WarehouseId.HasValue)
                query = query.Where(e => e.WarehouseId == filter.WarehouseId.Value);

            if (filter.StaffDetailId.HasValue)
                query = query.Where(e => e.StaffDetailId == filter.StaffDetailId.Value);

            if (!string.IsNullOrWhiteSpace(filter.Status))
            {
                var st = filter.Status.Trim().ToLower();
                query = query.Where(e => e.Status != null && e.Status.ToLower().Contains(st));
            }

            if (!string.IsNullOrWhiteSpace(filter.Comments))
            {
                var cm = filter.Comments.Trim().ToLower();
                query = query.Where(e => e.Comments != null && e.Comments.ToLower().Contains(cm));
            }

            // --- Sorting ---
            if (!string.IsNullOrWhiteSpace(filter.SortBy))
            {
                bool desc = filter.IsDescending;
                switch (filter.SortBy.Trim().ToLower())
                {
                    case "warehouseid":
                        query = desc
                            ? query.OrderByDescending(e => e.WarehouseId)
                            : query.OrderBy(e => e.WarehouseId);
                        break;
                    case "warehousename":
                        query = desc
                            ? query.OrderByDescending(e => e.Warehouse.WarehouseName)
                            : query.OrderBy(e => e.Warehouse.WarehouseName);
                        break;
                    case "allocatedquantity":
                        query = desc
                            ? query.OrderByDescending(e => e.AllocatedQuantity)
                            : query.OrderBy(e => e.AllocatedQuantity);
                        break;
                    case "status":
                        query = desc
                            ? query.OrderByDescending(e => e.Status)
                            : query.OrderBy(e => e.Status);
                        break;
                    case "comments":
                        query = desc
                            ? query.OrderByDescending(e => e.Comments)
                            : query.OrderBy(e => e.Comments);
                        break;
                    case "staffname":
                        query = desc
                            ? query.OrderByDescending(e => e.StaffDetail != null
                                ? e.StaffDetail.Account.FullName
                                : string.Empty)
                            : query.OrderBy(e => e.StaffDetail != null
                                ? e.StaffDetail.Account.FullName
                                : string.Empty);
                        break;
                    default:
                        query = query.OrderBy(e => e.DispatchStoreDetailId);
                        break;
                }
            }
            else
            {
                query = query.OrderBy(e => e.DispatchStoreDetailId);
            }

            // --- Paging ---
            var totalCount = await query.CountAsync();
            var items = await query
                .ProjectTo<ExportDetailDto>(_mapper.ConfigurationProvider)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResponseDTO<ExportDetailDto>(items, totalCount, page, pageSize);
        }

        public async Task<PaginatedResponseDTO<StoreExportStoreDetailDto>> GetStoreExportStoreDetailByStaffDetailAsync(
    StoreExportStoreDetailFilterDtO filter)
        {
            var query = _context.StoreExportStoreDetails
                .AsNoTracking()
                .Where(s => s.StaffDetailId == filter.StaffDetailId);

            if (!string.IsNullOrWhiteSpace(filter.Status))
            {
                var pattern = $"%{filter.Status}%";
                query = query.Where(s => EF.Functions.Like(s.Status, pattern));
            }

            var projected = query.Select(s => new StoreExportStoreDetailDto
            {
                DispatchId = s.DispatchDetail.Dispatch.DispatchId,
                DispatchStoreDetailId = s.DispatchStoreDetailId,
                WarehouseId = s.WarehouseId,
                WarehouseName = s.Warehouse.WarehouseName,
                ActualQuantity = s.ActualQuantity,
                AllocatedQuantity = s.AllocatedQuantity,
                Status = s.Status,
                Comments = s.Comments,
                StaffDetailId = s.StaffDetailId,
                DispatchDetailId = s.DispatchDetailId,

            });

            // Apply sorting
            bool desc = filter.IsDescending;
            projected = filter.SortBy?.Trim().ToLower() switch
            {
                "warehouseid" => desc ? projected.OrderByDescending(x => x.WarehouseId) : projected.OrderBy(x => x.WarehouseId),
                "actualquantity" => desc ? projected.OrderByDescending(x => x.ActualQuantity) : projected.OrderBy(x => x.ActualQuantity),
                "allocatedquantity" => desc ? projected.OrderByDescending(x => x.AllocatedQuantity) : projected.OrderBy(x => x.AllocatedQuantity),
                "status" => desc ? projected.OrderByDescending(x => x.Status) : projected.OrderBy(x => x.Status),
                "comments" => desc ? projected.OrderByDescending(x => x.Comments) : projected.OrderBy(x => x.Comments),
                "dispatchdetailid" => desc ? projected.OrderByDescending(x => x.DispatchDetailId) : projected.OrderBy(x => x.DispatchDetailId),

                _ => desc ? projected.OrderByDescending(x => x.DispatchStoreDetailId) : projected.OrderBy(x => x.DispatchStoreDetailId),
            };

            // Count and paginate
            var total = await projected.CountAsync();
            var items = await projected
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return new PaginatedResponseDTO<StoreExportStoreDetailDto>(items, total, filter.Page, filter.PageSize);
        }

        

    }
}



