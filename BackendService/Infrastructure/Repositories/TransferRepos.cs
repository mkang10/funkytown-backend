using Domain.DTO.Response;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class TransferRepos : ITransferRepos
    {
        private readonly FtownContext _context;
        public TransferRepos(FtownContext context)
        {
            _context = context;
        }

        public async Task<Transfer?> GetByIdWithDetailsAsync(int transferId)
        {
            return await _context.Transfers
                .Include(t => t.TransferDetails)
                    .ThenInclude(td => td.Variant)
                        .ThenInclude(v => v.Product)
                .Include(t => t.TransferDetails)
                    .ThenInclude(td => td.Variant)
                        .ThenInclude(v => v.Color)
                .Include(t => t.TransferDetails)
                    .ThenInclude(td => td.Variant)
                        .ThenInclude(v => v.Size)
                .FirstOrDefaultAsync(t => t.TransferOrderId == transferId);
        }


        public void Add(Transfer transfer)
        {
            _context.Transfers.Add(transfer);
        }

        public Task SaveChangesAsync()
        {
            return _context.SaveChangesAsync();
        }

        public async Task<PaginatedResponseDTO<TransferDto>> GetAllWithPagingAsync(
     int page,
     int pageSize,
     string? filter,
     CancellationToken cancellationToken = default)
        {
            var query = _context.Transfers
                .AsNoTracking()
                .Select(t => new TransferDto
                {
                    TransferOrderId = t.TransferOrderId,
                    ImportId = t.ImportId,
                    ImportReferenceNumber = t.Import.ReferenceNumber,
                    DispatchId = t.DispatchId,
                    DispatchReferenceNumber = t.Dispatch.ReferenceNumber,
                    CreatedBy = t.CreatedBy,
                    CreatedByName = _context.Accounts
                                                .Where(a => a.AccountId == t.CreatedBy)
                                                .Select(a => a.FullName)
                                                .FirstOrDefault()!,
                    CreatedDate = t.CreatedDate,
                    Status = t.Status,
                    Remarks = t.Remarks,
                    OriginalTransferOrderId = t.OriginalTransferOrderId
                });

            // Áp dụng filter như cũ
            if (!string.IsNullOrWhiteSpace(filter))
            {
                var norm = filter.Trim().ToLower();
                query = query.Where(t =>
                    t.Status.ToLower().Contains(norm) ||
                    (t.Remarks != null && t.Remarks.ToLower().Contains(norm)) ||
                    t.ImportReferenceNumber.ToLower().Contains(norm) ||
                    t.DispatchReferenceNumber.ToLower().Contains(norm) ||
                    t.CreatedByName.ToLower().Contains(norm)
                );
            }

            // --- Thêm dòng này để default sort theo TransferOrderId DESC ---
            query = query.OrderByDescending(t => t.TransferOrderId);

            // Tiếp tục count và paging
            var total = await query.CountAsync(cancellationToken);
            var data = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new PaginatedResponseDTO<TransferDto>(data, total, page, pageSize);
        }

        public Task<Transfer> GetJSONTransferOrderById(int id)
        {
            var data = _context.Transfers.
                Include(o => o.TransferDetails).
                Include(o => o.Import).
                    ThenInclude(od => od.ImportDetails).
                    ThenInclude(oc => oc.ImportStoreDetails).ThenInclude(oc => oc.Warehouse).
  Include(o => o.Import).
                    ThenInclude(od => od.ImportDetails).
                    ThenInclude(oc => oc.ImportStoreDetails).ThenInclude(oc => oc.StaffDetail).ThenInclude(oc => oc.Account).
                    Include(o => o.Import).
                    ThenInclude(od => od.ImportDetails).
                    ThenInclude(oc => oc.ImportStoreDetails).ThenInclude(oc => oc.HandleByNavigation).ThenInclude(oc => oc.Account).
                Include(O => O.Dispatch).
                Include(O => O.Dispatch).
                    ThenInclude(od => od.DispatchDetails).
                    ThenInclude(oc => oc.StoreExportStoreDetails).
                     Include(o => o.Import).
                    ThenInclude(od => od.ImportDetails).
                    ThenInclude(oc => oc.ProductVariant).ThenInclude(oc => oc.Product).
                     Include(o => o.Import).
                    ThenInclude(od => od.ImportDetails).
                    ThenInclude(oc => oc.ProductVariant).ThenInclude(oc => oc.Size).
                     Include(o => o.Import).
                    ThenInclude(od => od.ImportDetails).
                    ThenInclude(oc => oc.ProductVariant).ThenInclude(oc => oc.Color).
                    FirstOrDefaultAsync(a => a.TransferOrderId == id);
            return data;
        }
        public async Task AddAsync(Transfer transfer)
            => await _context.Transfers.AddAsync(transfer);


        public async Task UpdateAsync(Transfer entity)
        {
            // Đánh dấu entity đã thay đổi
            _context.Transfers.Update(entity);
            // Lưu thay đổi xuống CSDL
            await _context.SaveChangesAsync();
        }

        public async Task AddRangeAsync(IEnumerable<TransferDetail> entities)
        {
            // EF Core đã hỗ trợ AddRangeAsync
            await _context.Set<TransferDetail>().AddRangeAsync(entities);
        }
    }


}