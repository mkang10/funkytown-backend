using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class TransferDetail
{
    public int TransferOrderDetailId { get; set; }

    public int TransferOrderId { get; set; }

    public int VariantId { get; set; }

    public int Quantity { get; set; }

    public int? DeliveredQuantity { get; set; }

    public virtual Transfer TransferOrder { get; set; } = null!;

    public virtual ProductVariant Variant { get; set; } = null!;
}
