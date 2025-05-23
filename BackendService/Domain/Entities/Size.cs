using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class Size
{
    public int SizeId { get; set; }

    public string SizeName { get; set; } = null!;

    public string? SizeDescription { get; set; }

    public DateTime? CreatedDate { get; set; }

    public virtual ICollection<ProductVariant> ProductVariants { get; set; } = new List<ProductVariant>();
}
