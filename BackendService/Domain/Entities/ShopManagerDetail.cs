using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class ShopManagerDetail
{
    public int ShopManagerDetailId { get; set; }

    public int AccountId { get; set; }

    public DateTime? ManagedDate { get; set; }

    public int? YearsOfExperience { get; set; }

    public string? ManagerCertifications { get; set; }

    public string? OfficeContact { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual ICollection<ImportStoreDetail> ImportStoreDetails { get; set; } = new List<ImportStoreDetail>();

    public virtual ICollection<StoreExportStoreDetail> StoreExportStoreDetails { get; set; } = new List<StoreExportStoreDetail>();

    public virtual ICollection<Warehouse> Warehouses { get; set; } = new List<Warehouse>();
}
