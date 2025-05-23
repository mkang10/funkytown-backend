using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class Dispatch
{
    public int DispatchId { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public string Status { get; set; } = null!;

    public string? ReferenceNumber { get; set; }

    public string? Remarks { get; set; }

    public int? OriginalId { get; set; }

    public DateTime? CompletedDate { get; set; }

    public virtual Account CreatedByNavigation { get; set; } = null!;

    public virtual ICollection<DispatchDetail> DispatchDetails { get; set; } = new List<DispatchDetail>();

    public virtual ICollection<Transfer> Transfers { get; set; } = new List<Transfer>();
}
