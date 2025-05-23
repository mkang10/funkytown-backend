using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class Order
{
    public int OrderId { get; set; }

    public int AccountId { get; set; }

    public int? WareHouseId { get; set; }

    public int? ShippingAddressId { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? Status { get; set; }

    public decimal? OrderTotal { get; set; }

    public decimal? ShippingCost { get; set; }

    public string? Ghnid { get; set; }

    public string? DeliveryMethod { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public string Address { get; set; } = null!;

    public string City { get; set; } = null!;

    public string District { get; set; } = null!;

    public string? Country { get; set; }

    public string? Province { get; set; }

    public bool? IsFeedback { get; set; }

    public DateTime? CompletedDate { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual ICollection<DeliveryTracking> DeliveryTrackings { get; set; } = new List<DeliveryTracking>();

    public virtual ICollection<OrderAssignment> OrderAssignments { get; set; } = new List<OrderAssignment>();

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<ReturnOrder> ReturnOrders { get; set; } = new List<ReturnOrder>();

    public virtual ShippingAddress? ShippingAddress { get; set; }

    public virtual Warehouse? WareHouse { get; set; }
}
