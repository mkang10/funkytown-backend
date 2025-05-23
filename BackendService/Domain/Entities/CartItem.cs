using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class CartItem
{
    public int CartItemId { get; set; }

    public int CartId { get; set; }

    public int ProductVariantId { get; set; }

    public int Quantity { get; set; }

    public virtual ShoppingCart Cart { get; set; } = null!;

    public virtual ProductVariant ProductVariant { get; set; } = null!;
}
