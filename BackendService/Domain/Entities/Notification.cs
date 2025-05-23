using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class Notification
{
    public int NotificationId { get; set; }

    public int AccountId { get; set; }

    public string? Content { get; set; }

    public string? NotificationType { get; set; }

    public bool? IsRead { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string Title { get; set; } = null!;

    public int? TargetId { get; set; }

    public string? TargetType { get; set; }

    public DateTime? ExpirationDate { get; set; }

    public virtual Account Account { get; set; } = null!;
}
