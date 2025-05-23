using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class Import
{
    public int ImportId { get; set; }

    public int CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? Status { get; set; }

    public string? ReferenceNumber { get; set; }

    public decimal? TotalCost { get; set; }

    public DateTime? ApprovedDate { get; set; }

    public DateTime? CompletedDate { get; set; }

    public int? OriginalImportId { get; set; }

    public string? ImportType { get; set; }

    public bool? IsUrgent { get; set; }

    public virtual Account CreatedByNavigation { get; set; } = null!;

    public virtual ICollection<ImportDetail> ImportDetails { get; set; } = new List<ImportDetail>();

    public virtual ICollection<Transfer> Transfers { get; set; } = new List<Transfer>();
}
