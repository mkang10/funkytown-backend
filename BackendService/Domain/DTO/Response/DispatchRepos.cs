public class DispatchResponseDto
{
    public int DispatchId { get; set; }
    public string Status { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Remarks { get; set; }
    public DateTime CreatedDate { get; set; }
    public string CreatedByName { get; set; }
    public DateTime? CompletedDate { get; set; }

    public List<DispatchDetailDto> DispatchDetails { get; set; } = new();
}

public class DispatchDetailDto
{
    public int DispatchDetailId { get; set; }
    public int VariantId { get; set; }
    public string ProductName { get; set; }
    public int Quantity { get; set; }

    public List<ExportDetailDto> ExportDetails { get; set; } = new();
}

public class ExportDetailDto
{
    public int DispatchStoreDetailId { get; set; }
    public int DispatchId { get; set; }

    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; }
    public int? StaffDetailId { get; set; }
    public string? StaffName { get; set; }
    public int AllocatedQuantity { get; set; }
    public int? ActualQuantity { get; set; }
    public string? Status { get; set; }
    public string? Comments { get; set; }
}
