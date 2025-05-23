using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class ShippingAddress
{
    public int AddressId { get; set; }

    public int AccountId { get; set; }

    public string Address { get; set; } = null!;

    public string? City { get; set; }

    public string? Country { get; set; }

    public bool? IsDefault { get; set; }

    public string RecipientName { get; set; } = null!;

    public string RecipientPhone { get; set; } = null!;

    public string? Province { get; set; }

    public string? District { get; set; }

    public string? Email { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
