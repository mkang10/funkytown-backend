using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Response
{
    public class CartItemResponse
    {
        public int ProductVariantId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string? ImagePath { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal DiscountedPrice { get; set; } 
        public string? PromotionTitle { get; set; }
        public string? Message { get; set; }
        public bool IsValid => string.IsNullOrEmpty(Message);
    }
}
