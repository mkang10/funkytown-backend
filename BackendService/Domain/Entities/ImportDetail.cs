using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class ImportDetail
{
    public int ImportDetailId { get; set; }

    public int ImportId { get; set; }

    public int ProductVariantId { get; set; }

    public int Quantity { get; set; }

    public decimal? CostPrice { get; set; }

    public virtual Import Import { get; set; } = null!;

    public virtual ICollection<ImportStoreDetail> ImportStoreDetails { get; set; } = new List<ImportStoreDetail>();

    public virtual ProductVariant ProductVariant { get; set; } = null!;
}
