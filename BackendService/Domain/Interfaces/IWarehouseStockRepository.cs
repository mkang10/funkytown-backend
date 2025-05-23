using Domain.DTO.Response;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IWareHousesStockRepository
    {
        Task<PaginatedResponseDTO<WarehouseStockDto>> GetAllWareHouse(int page, int pageSize, CancellationToken cancellationToken = default);

        Task<int> GetStockQuantityAsync(int warehouseId, int variantId);
        Task<int> GetTotalStockByVariantAsync(int variantId);
        Task<bool> UpdateStockAfterOrderAsync(int warehouseId, List<(int VariantId, int Quantity)> stockUpdates);
        Task<bool> RestoreStockAfterCancelAsync(int warehouseId, List<(int VariantId, int Quantity)> stockUpdates);
        Task<WareHousesStock?> GetByIdWithDetailsAsync(int id);
        Task<IEnumerable<WareHousesStock>> GetByWarehouseIdAsync(int warehouseId);
        Task<bool> HasStockAsync(int ProductId, int sizeId, int ColorId);

        Task<IEnumerable<WareHousesStock>> GetByWarehouseAsync(int warehouseId);
        Task<WareHousesStock?> GetByWarehouseAndVariantAsync(int warehouseId, int variantId);
        Task<IEnumerable<WareHousesStock>> GetAllByVariantAsync(int variantId);
        Task UpdateAsync(WareHousesStock stock);
        Task UpdateWarehouseStockAsync(Import import, int staffId);
        Task UpdateWarehouseStockAsync(
    Import import,
    int staffId,
    List<int> confirmedStoreDetailIds);
        Task UpdateDispatchWarehouseStockAsync(Dispatch dispatch, int staffId);

        Task UpdateDispatchWarehouseStockAsync(
   Dispatch dispatch,
   int staffId,
   List<int> confirmedStoreDetailIds);
        Task SaveChangesAsync();

        Task UpdateWarehouseStockForSingleDispatchDetailAsync(StoreExportStoreDetail storeDetail, int productVariantId, int staffId);


        Task UpdateWarehouseStockForSingleDetailAsync(ImportStoreDetail storeDetail, int productVariantId, int staffId);
    }

}
