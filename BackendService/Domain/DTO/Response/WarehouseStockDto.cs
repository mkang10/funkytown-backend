using System;
using System.Collections.Generic;

namespace Domain.DTOs
{
    public class WarehouseStockAuditDto
    {
        public int AuditId { get; set; }
        public string Action { get; set; } = null!;
        public int QuantityChange { get; set; }
        public DateTime ActionDate { get; set; }
        public int? ChangedBy { get; set; }
        public string? changedByName { get; set; }

        public string? Note { get; set; }
    }

    public class WarehouseStockDto
    {
        public int WareHouseStockId { get; set; }
        public int VariantId { get; set; }
        public string VariantName { get; set; } = null!;
        public int StockQuantity { get; set; }
        public int WareHouseId { get; set; }
        public string WareHouseName { get; set; } = null!;
        public List<WarehouseStockAuditDto> AuditHistory { get; set; } = new();
    }
}