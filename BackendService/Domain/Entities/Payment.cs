﻿using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class Payment
{
    public int PaymentId { get; set; }

    public int OrderId { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public string? PaymentStatus { get; set; }

    public DateTime? TransactionDate { get; set; }

    public decimal Amount { get; set; }

    public long? OrderCode { get; set; }

    public virtual Order Order { get; set; } = null!;
}
