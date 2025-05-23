using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class Promotion
{
    public int PromotionId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string DiscountType { get; set; } = null!;

    public decimal DiscountValue { get; set; }

    public decimal? MinOrderAmount { get; set; }

    public decimal? MaxDiscountAmount { get; set; }

    public string ApplyTo { get; set; } = null!;

    public string? ApplyValue { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public string Status { get; set; } = null!;
}
