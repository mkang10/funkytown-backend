using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class ImportStoreDetail
{
    public int? ActualReceivedQuantity { get; set; }

    public int AllocatedQuantity { get; set; }

    public string? Status { get; set; }

    public string? Comments { get; set; }

    public int? StaffDetailId { get; set; }

    public int ImportDetailId { get; set; }

    public int ImportStoreId { get; set; }

    public int? WarehouseId { get; set; }

    public int? HandleBy { get; set; }

    public virtual ShopManagerDetail? HandleByNavigation { get; set; }

    public virtual ImportDetail ImportDetail { get; set; } = null!;

    public virtual StaffDetail? StaffDetail { get; set; }

    public virtual Warehouse? Warehouse { get; set; }
}
