using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class StoreExportStoreDetail
{
    public int WarehouseId { get; set; }

    public int AllocatedQuantity { get; set; }

    public string? Status { get; set; }

    public string? Comments { get; set; }

    public int? StaffDetailId { get; set; }

    public int? DispatchDetailId { get; set; }

    public int? HandleBy { get; set; }

    public int DispatchStoreDetailId { get; set; }

    public int? ActualQuantity { get; set; }

    public int? DestinationId { get; set; }

    public virtual DispatchDetail? DispatchDetail { get; set; }

    public virtual ShopManagerDetail? HandleByNavigation { get; set; }

    public virtual StaffDetail? StaffDetail { get; set; }

    public virtual Warehouse Warehouse { get; set; } = null!;
}
