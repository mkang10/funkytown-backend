using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class StaffDetail
{
    public int StaffDetailId { get; set; }

    public int AccountId { get; set; }

    public DateTime? JoinDate { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual ICollection<ImportStoreDetail> ImportStoreDetails { get; set; } = new List<ImportStoreDetail>();

    public virtual ICollection<StoreExportStoreDetail> StoreExportStoreDetails { get; set; } = new List<StoreExportStoreDetail>();

    public virtual ICollection<WarehouseStaff> WarehouseStaffs { get; set; } = new List<WarehouseStaff>();
}
