using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class Product
{
    public int ProductId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int? CategoryId { get; set; }

    public string? ImagePath { get; set; }

    public string? Origin { get; set; }

    public string? Model { get; set; }

    public string? Occasion { get; set; }

    public string? Style { get; set; }

    public string? Material { get; set; }

    public string? Status { get; set; }

    public virtual Category? Category { get; set; }

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();

    public virtual ICollection<ProductVariant> ProductVariants { get; set; } = new List<ProductVariant>();
}
