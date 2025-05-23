using Domain.DTO.Request;
using Domain.DTO.Response;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Domain.DTO.Request.StoreExportStoreDetailReq;

namespace Domain.Interfaces
{
    public interface IDispatchRepos { 
        void Add(Dispatch dispatch);
        Task SaveChangesAsync();
        Task<Dispatch?> GetDispatchByTransferIdAsync(int transferId);
        Task<int> GetApprovedOutboundQuantityAsync(int warehouseId, int variantId);
        // duc anh
        public Task<Dispatch> GetJSONDispatchById(int id);

        Task<StoreExportStoreDetail> GetStoreExportStoreDetailById(int importId);
        Task ReloadAsync(Dispatch dispatch);

        Task AddAsync(Dispatch dispatch);
        Task<Dispatch?> GetByIdAssignAsync(int dispatchId);
        Task<Dispatch?> GetByIdAsync(int dispatchId);
        Task<List<Dispatch>> GetAllByOriginalDispatchIdAsync(int originalDispatchId);
        Task<Dispatch?> GetByIdDispatchAssignAsync(int dispatchId);

        Task<PaginatedResponseDTO<DispatchResponseDto>> GetAllDispatchAsync(int page, int pageSize, DispatchFilterDto filter);

        Task UpdateAsync(Dispatch dispatch);

        Task<PaginatedResponseDTO<ExportDetailDto>> GetAllExportStoreDetailsAsync(
              int page,
              int pageSize,
              StoreExportStoreDetailFilterDto filter);

        Task<PaginatedResponseDTO<StoreExportStoreDetailDto>> GetStoreExportStoreDetailByStaffDetailAsync(
    StoreExportStoreDetailFilterDtO filter);

    }
}
