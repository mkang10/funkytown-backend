using Application.UseCases;
using Domain.DTO.Response;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{

    public class DashboardHandler
    {
        private readonly IDashboardRepository _repo;
        public DashboardHandler(IDashboardRepository repo)
        {
            _repo = repo;
        }

        public async Task<DashboardDto> GetDashboardAsync(string? statusFilter = null)
        {
            var dto = new DashboardDto
            {
                ImportStatusCounts = await _repo.GetImportStatusCountsAsync(statusFilter),
                DispatchStatusCounts = await _repo.GetDispatchStatusCountsAsync(statusFilter),
                TransferStatusCounts = await _repo.GetTransferStatusCountsAsync(statusFilter),
                TotalImports = await _repo.GetImportTotalCountAsync(statusFilter),
                TotalImportCost = await _repo.GetImportTotalCostAsync(statusFilter),
                TotalDispatches = await _repo.GetDispatchTotalCountAsync(statusFilter),
                TotalTransfers = await _repo.GetTransferTotalCountAsync(statusFilter)
            };
            return dto;
        }

    }
}

