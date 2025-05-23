using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IWareHouseStockAuditRepository
    {
        Task AddWareHouseStockAuditAsync(int warehouseStockId, string action, int quantityChange, int? changedBy, string? note);
        Task<Dictionary<int, int>> GetWarehouseStockMapAsync(List<int> productVariantIds, int warehouseId);
    }


}
