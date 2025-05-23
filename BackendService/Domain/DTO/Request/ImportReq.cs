
public class ImportFilterDto
{
    public string? Status { get; set; }
    public string? ReferenceNumber { get; set; }
    public int? HandleBy { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? SortBy { get; set; } = "ImportId"; // Default sorting field
    public bool IsDescending { get; set; } = false;   // Default ascending
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}


