using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class ReturnOrder
{
    public int ReturnOrderId { get; set; }

    public int OrderId { get; set; }

    public int AccountId { get; set; }

    public string? Email { get; set; }

    public decimal TotalRefundAmount { get; set; }

    public string ReturnReason { get; set; } = null!;

    public string ReturnOption { get; set; } = null!;

    public string ReturnDescription { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public string? BankName { get; set; }

    public string? BankAccountNumber { get; set; }

    public string? BankAccountName { get; set; }

    public string RefundMethod { get; set; } = null!;

    public string? ReturnImages { get; set; }

    public int? HandledBy { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual ICollection<ReturnOrderItem> ReturnOrderItems { get; set; } = new List<ReturnOrderItem>();
}
