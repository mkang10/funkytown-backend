using Application.Enums;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class WareHouseStockAuditHandler
    {
        private readonly IWareHouseStockAuditRepository _warehouseStockAuditRepository;

        public WareHouseStockAuditHandler(IWareHouseStockAuditRepository warehouseStockAuditRepository)
        {
            _warehouseStockAuditRepository = warehouseStockAuditRepository;
        }

        public async Task LogDecreaseStockAsync(
            int warehouseStockId,
            int quantityReduced,
            int? changedBy,
            string? note = null)
        {
            await _warehouseStockAuditRepository.AddWareHouseStockAuditAsync(
                warehouseStockId: warehouseStockId,
                action: WareHouseStockAction.Decrease.ToString(), // convert enum to string
                quantityChange: -Math.Abs(quantityReduced),
                changedBy: changedBy,
                note: note
            );
        }
    }
}
