using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class ProductVariant
{
    public int VariantId { get; set; }

    public int ProductId { get; set; }

    public int? SizeId { get; set; }

    public int? ColorId { get; set; }

    public decimal Price { get; set; }

    public string? ImagePath { get; set; }

    public string? Sku { get; set; }

    public string? Barcode { get; set; }

    public decimal? Weight { get; set; }

    public string? Status { get; set; }

    public int? MaxStocks { get; set; }

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual Color? Color { get; set; }

    public virtual ICollection<DispatchDetail> DispatchDetails { get; set; } = new List<DispatchDetail>();

    public virtual ICollection<ImportDetail> ImportDetails { get; set; } = new List<ImportDetail>();

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual Product Product { get; set; } = null!;

    public virtual ICollection<ReturnOrderItem> ReturnOrderItems { get; set; } = new List<ReturnOrderItem>();

    public virtual Size? Size { get; set; }

    public virtual ICollection<TransferDetail> TransferDetails { get; set; } = new List<TransferDetail>();

    public virtual ICollection<WareHousesStock> WareHousesStocks { get; set; } = new List<WareHousesStock>();
}
