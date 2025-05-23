// Domain/DTOs/OrderDetailDto.cs
public class OrderDetailDto
{
    public int OrderDetailId { get; set; }
    public int ProductVariantId { get; set; }
    public string ProductName { get; set; } = null!;     // từ ProductVariant → Product.Name
    public string? VariantName { get; set; }             // nếu có trường VariantName
    public int Quantity { get; set; }
    public decimal PriceAtPurchase { get; set; }
    public decimal? DiscountApplied { get; set; }
    public string? SizeName { get; set; }                 // MỚI: từ ProductVariant → Size.Name
    public string? ColorName { get; set; }
}

// Domain/DTOs/OrderDto.cs
public class OrderDto
{
    public int OrderId { get; set; }
    public DateTime? CreatedDate { get; set; }
    public string? Status { get; set; }
    public decimal? OrderTotal { get; set; }
    public decimal? ShippingCost { get; set; }

    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string City { get; set; } = null!;
    public string District { get; set; } = null!;
    public string? Country { get; set; }
    public string? Province { get; set; }

    public List<OrderDetailDto> OrderDetails { get; set; } = new();
}

// Domain/DTOs/OrderAssignmentDto.cs
public class OrderAssignmentDto
{
    public int AssignmentId { get; set; }
    public int ShopManagerId { get; set; }
    public int? StaffId { get; set; }
    public DateTime? AssignmentDate { get; set; }
    public string? Comments { get; set; }

    public OrderDto Order { get; set; } = null!;
}
