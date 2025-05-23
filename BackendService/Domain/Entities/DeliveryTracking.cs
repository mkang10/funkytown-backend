using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class DeliveryTracking
{
    public int TrackingId { get; set; }

    public int OrderId { get; set; }

    public string? CurrentLocation { get; set; }

    public string? Status { get; set; }

    public DateTime? LastUpdated { get; set; }

    public DateTime? EstimatedDeliveryDate { get; set; }

    public virtual Order Order { get; set; } = null!;
}
