using Domain.DTO.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IDashboardRepository
    {
        Task<List<StatusCountDto>> GetImportStatusCountsAsync(string? statusFilter = null);
        Task<List<StatusCountDto>> GetDispatchStatusCountsAsync(string? statusFilter = null);
        Task<List<StatusCountDto>> GetTransferStatusCountsAsync(string? statusFilter = null);

        Task<int> GetImportTotalCountAsync(string? statusFilter = null);
        Task<decimal> GetImportTotalCostAsync(string? statusFilter = null);
        Task<int> GetDispatchTotalCountAsync(string? statusFilter = null);
        Task<int> GetTransferTotalCountAsync(string? statusFilter = null);
    }
}
