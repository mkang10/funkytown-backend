using Domain.DTO.Request;
using Domain.DTO.Response;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IImportRepos
    {
        Task<PaginatedResponseDTO<ImportStoreDetailDtoStore>> GetStoreDetailsByStaffDetailAsync(
            ImportStoreDetailFilterDto filter);
        Task<List<Import>> GetAllByOriginalImportIdAsync(int originalImportId);
        Task<decimal?> GetLatestCostPriceAsync(int productId, int sizeId, int colorId);
        Task<Import?> GetByIdAssignAsync(int importId);
        Task<PaginatedResponseDTO<ImportStoreDetailDtoStore>> GetImportStoreDetailByStaffDetailAsync(ImportStoreDetailFilterDtO filter);
        Task<Import> AddAsync(Import import);
        Task<Import?> GetByIdAsync(int importId);
        Task UpdateAsync(Import import);
        Task<Account?> GetAccountByIdAsync(int accountId);

        Task<PagedResult<Import>> GetImportsAsync(InventoryImportFilterDto filter);

        Task SaveChangesAsync();
        Task<Import> GetImportByIdAsync(int importId);

        void Add(Import import);

        Task<Import> GetByIdAsyncWithDetails(int id);
        Task<(IEnumerable<Import>, int)> GetAllImportsAsync(ImportFilterDto filter, CancellationToken cancellationToken);

        Task<PaginatedResponseDTO<Warehouse>> GetAllWarehousesAsync(int page, int pageSize);

        Task<Warehouse> GetWareHouseByIdAsync(int id);
        Task<StaffDetail?> GetStaffDetailByIdAsync(int staffDetailId);

        Task<Import?> GetImportByTransferIdAsync(int transferId);

        // ======Duc Anh=======
        Task<ImportStoreDetail> GetImportStoreDetail(int importId);
        Task<Transfer?> GetTransferByIdAsync(int transferId);

        IQueryable<Transfer> QueryTransfers();

        IQueryable<ImportStoreDetail> QueryImportStoreDetailsByImportId(int importId);

       

        Task<PaginatedResponseDTO<ProductVariant>> GetAllAsync(int page, int pageSize, string search = null);
        Task ReloadAsync(Import import);

        Task<Transfer> GetTransferByImportIdAsync(int importId);

        Task<Transfer> GetTransferByDispatchIdAsync(int dispatchId);


        IQueryable<ImportDetail> QueryImportDetails();

        /// <summary>
        /// Kiểm tra xem Import có liên quan đến bất kỳ Transfer nào.
        /// </summary>
        Task<bool> HasTransferForImportAsync(int importId);

        /// <summary>
        /// Lấy đối tượng ProductVariant theo ID.
        /// </summary>
        Task<ProductVariant> GetProductVariantByIdAsync(int variantId);

    }
}

