using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class Transfer
{
    public int TransferOrderId { get; set; }

    public int ImportId { get; set; }

    public int DispatchId { get; set; }

    public int CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string Status { get; set; } = null!;

    public string? Remarks { get; set; }

    public int? OriginalTransferOrderId { get; set; }

    public virtual Dispatch Dispatch { get; set; } = null!;

    public virtual Import Import { get; set; } = null!;

    public virtual ICollection<TransferDetail> TransferDetails { get; set; } = new List<TransferDetail>();
}
