using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class WareHouseStockAuditRepository : IWareHouseStockAuditRepository
    {
        private readonly FtownContext _context;

        public WareHouseStockAuditRepository(FtownContext context)
        {
            _context = context;
        }

        public async Task AddWareHouseStockAuditAsync(int warehouseStockId, string action, int quantityChange, int? changedBy, string? note)
        {
            var audit = new WareHouseStockAudit
            {
                WareHouseStockId = warehouseStockId,
                Action = action,
                QuantityChange = quantityChange,
                ActionDate = DateTime.UtcNow,
                ChangedBy = changedBy,
                Note = note
            };

            _context.WareHouseStockAudits.Add(audit);
            await _context.SaveChangesAsync();
        }
        public async Task<Dictionary<int, int>> GetWarehouseStockMapAsync(List<int> productVariantIds, int warehouseId)
        {
            var stockList = await _context.WareHousesStocks
                .Where(s => s.WareHouseId == warehouseId && productVariantIds.Contains(s.VariantId))
                .ToListAsync();

            return stockList.ToDictionary(s => s.VariantId, s => s.WareHouseStockId);
        }

    }

}
