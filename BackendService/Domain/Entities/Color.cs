using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class Color
{
    public int ColorId { get; set; }

    public string ColorName { get; set; } = null!;

    public string? ColorCode { get; set; }

    public DateTime? CreatedDate { get; set; }

    public virtual ICollection<ProductVariant> ProductVariants { get; set; } = new List<ProductVariant>();
}
