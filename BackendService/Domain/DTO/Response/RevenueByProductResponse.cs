using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Response
{
    public class RevenueByProductResponse
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public string? CategoryName { get; set; }

        public int TotalQuantitySold { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AveragePrice { get; set; }

        public List<VariantRevenueItem> Variants { get; set; } = new();
    }
    public class VariantRevenueItem
    {
        public int VariantId { get; set; }
        public string? Size { get; set; }
        public string? Color { get; set; }

        public int QuantitySold { get; set; }
        public decimal Revenue { get; set; }
    }
}
