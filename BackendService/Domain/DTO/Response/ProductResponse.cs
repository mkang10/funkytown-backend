using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Response
{
    public class ProductResponse
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ImagePath { get; set; }
        public decimal Price { get; set; }
        public decimal DiscountedPrice { get; set; } // Giá sau khi áp dụng khuyến mãi
        public string? CategoryName { get; set; }
        public string? PromotionTitle { get; set; } // Tên khuyến mãi nếu có
        public bool? IsFavorite { get; set; }
        public List<string> Colors { get; set; } = new();
    }
    
}
