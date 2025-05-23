using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class WareHouseStockAudit
{
    public int AuditId { get; set; }

    public int WareHouseStockId { get; set; }

    public string Action { get; set; } = null!;

    public int QuantityChange { get; set; }

    public DateTime ActionDate { get; set; }

    public int? ChangedBy { get; set; }

    public string? Note { get; set; }

    public virtual Account? ChangedByNavigation { get; set; }

    public virtual WareHousesStock WareHouseStock { get; set; } = null!;
}
