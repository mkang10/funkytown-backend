using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class ReturnOrderItem
{
    public int ReturnOrderItemId { get; set; }

    public int ReturnOrderId { get; set; }

    public int ProductVariantId { get; set; }

    public int Quantity { get; set; }

    public decimal RefundPrice { get; set; }

    public virtual ProductVariant ProductVariant { get; set; } = null!;

    public virtual ReturnOrder ReturnOrder { get; set; } = null!;
}
