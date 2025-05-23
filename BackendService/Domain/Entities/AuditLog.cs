using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class AuditLog
{
    public int AuditLogId { get; set; }

    public string TableName { get; set; } = null!;

    public string RecordId { get; set; } = null!;

    public string Operation { get; set; } = null!;

    public DateTime ChangeDate { get; set; }

    public int? ChangedBy { get; set; }

    public string? ChangeData { get; set; }

    public string? Comment { get; set; }

    public virtual Account? ChangedByNavigation { get; set; }
}
