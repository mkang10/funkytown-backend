using Domain.DTO.Response;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly FtownContext _context;
        public DashboardRepository(FtownContext context)
        {
            _context = context;
        }

        public async Task<List<StatusCountDto>> GetImportStatusCountsAsync(string? statusFilter = null)
        {
            var query = _context.Imports.AsNoTracking();
            if (!string.IsNullOrEmpty(statusFilter))
                query = query.Where(i => i.Status == statusFilter);

            return await query
                .GroupBy(i => i.Status ?? "Unknown")
                .Select(g => new StatusCountDto { Status = g.Key, Count = g.Count() })
                .ToListAsync();
        }

        public async Task<List<StatusCountDto>> GetDispatchStatusCountsAsync(string? statusFilter = null)
        {
            var query = _context.Dispatches.AsNoTracking();
            if (!string.IsNullOrEmpty(statusFilter))
                query = query.Where(d => d.Status == statusFilter);

            return await query
                .GroupBy(d => d.Status)
                .Select(g => new StatusCountDto { Status = g.Key, Count = g.Count() })
                .ToListAsync();
        }

        public async Task<List<StatusCountDto>> GetTransferStatusCountsAsync(string? statusFilter = null)
        {
            var query = _context.Transfers.AsNoTracking();
            if (!string.IsNullOrEmpty(statusFilter))
                query = query.Where(t => t.Status == statusFilter);

            return await query
                .GroupBy(t => t.Status)
                .Select(g => new StatusCountDto { Status = g.Key, Count = g.Count() })
                .ToListAsync();
        }

        public async Task<int> GetImportTotalCountAsync(string? statusFilter = null)
        {
            var query = _context.Imports.AsNoTracking();
            if (!string.IsNullOrEmpty(statusFilter))
                query = query.Where(i => i.Status == statusFilter);
            return await query.CountAsync();
        }

        public async Task<decimal> GetImportTotalCostAsync(string? statusFilter = null)
        {
            var query = _context.Imports.AsNoTracking();
            if (!string.IsNullOrEmpty(statusFilter))
                query = query.Where(i => i.Status == statusFilter);
            return await query.SumAsync(i => i.TotalCost ?? 0);
        }

        public async Task<int> GetDispatchTotalCountAsync(string? statusFilter = null)
        {
            var query = _context.Dispatches.AsNoTracking();
            if (!string.IsNullOrEmpty(statusFilter))
                query = query.Where(d => d.Status == statusFilter);
            return await query.CountAsync();
        }

        public async Task<int> GetTransferTotalCountAsync(string? statusFilter = null)
        {
            var query = _context.Transfers.AsNoTracking();
            if (!string.IsNullOrEmpty(statusFilter))
                query = query.Where(t => t.Status == statusFilter);
            return await query.CountAsync();
        }
    }
}