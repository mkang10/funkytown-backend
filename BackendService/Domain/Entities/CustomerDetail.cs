using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class CustomerDetail
{
    public int CustomerDetailId { get; set; }

    public int AccountId { get; set; }

    public int? LoyaltyPoints { get; set; }

    public string? MembershipLevel { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string? Gender { get; set; }

    public string? CustomerType { get; set; }

    public string? PreferredPaymentMethod { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual ICollection<CustomerStyle> CustomerStyles { get; set; } = new List<CustomerStyle>();
}
