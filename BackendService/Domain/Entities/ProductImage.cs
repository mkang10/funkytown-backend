using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class ProductImage
{
    public int ProductImageId { get; set; }

    public int ProductId { get; set; }

    public string ImagePath { get; set; } = null!;

    public bool IsMain { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual Product Product { get; set; } = null!;
}
