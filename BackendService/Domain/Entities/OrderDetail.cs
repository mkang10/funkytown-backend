using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class OrderDetail
{
    public int OrderDetailId { get; set; }

    public int OrderId { get; set; }

    public int ProductVariantId { get; set; }

    public int Quantity { get; set; }

    public decimal PriceAtPurchase { get; set; }

    public decimal? DiscountApplied { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual ProductVariant ProductVariant { get; set; } = null!;
}
