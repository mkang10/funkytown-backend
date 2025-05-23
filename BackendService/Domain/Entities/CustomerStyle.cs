using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class CustomerStyle
{
    public int CustomerStyleId { get; set; }

    public int CustomerDetailId { get; set; }

    public int StyleId { get; set; }

    public int Point { get; set; }

    public int ClickCount { get; set; }

    public bool IsFromPreference { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? LastUpdatedDate { get; set; }

    public virtual CustomerDetail CustomerDetail { get; set; } = null!;

    public virtual Style Style { get; set; } = null!;
}
