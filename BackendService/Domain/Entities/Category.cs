using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class Category
{
    public int CategoryId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int? ParentCategoryId { get; set; }

    public bool? IsActive { get; set; }

    public int? DisplayOrder { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
