using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Response
{
    public class OrderItemResponse
    {
        public int OrderDetailId { get; set; }
        public int ProductVariantId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public string ImageUrl { get; set; }
        public string Size { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal PriceAtPurchase { get; set; }
        public decimal DiscountApplied { get; set; }
    }
}
