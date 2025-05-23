using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Response
{
    public class TopSellingProductResponse
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public int QuantitySold { get; set; }
        public decimal Revenue { get; set; }

        public string? ImagePath { get; set; }
        public decimal Price { get; set; }
        public decimal DiscountedPrice { get; set; }

        public string? CategoryName { get; set; }
        public string? PromotionTitle { get; set; }
        public List<string> Colors { get; set; } = new();
    }

}
