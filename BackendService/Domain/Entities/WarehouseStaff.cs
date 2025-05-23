using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class WarehouseStaff
{
    public int WarehouseStaffId { get; set; }

    public int WarehouseId { get; set; }

    public int StaffDetailId { get; set; }

    public string Role { get; set; } = null!;

    public virtual StaffDetail StaffDetail { get; set; } = null!;

    public virtual Warehouse Warehouse { get; set; } = null!;
}
