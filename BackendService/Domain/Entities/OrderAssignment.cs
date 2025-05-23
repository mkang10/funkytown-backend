using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class OrderAssignment
{
    public int AssignmentId { get; set; }

    public int OrderId { get; set; }

    public int ShopManagerId { get; set; }

    public int? StaffId { get; set; }

    public DateTime? AssignmentDate { get; set; }

    public string? Comments { get; set; }

    public virtual Order Order { get; set; } = null!;
}
