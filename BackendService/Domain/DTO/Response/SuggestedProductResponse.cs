using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Response
{
    public class SuggestedProductResponse
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty; // 🔥 Đổi ProductName thành Name
        public string? ImagePath { get; set; }
        public decimal Price { get; set; }
        public decimal DiscountedPrice { get; set; } // ✅ Bổ sung trường DiscountedPrice (vì ProductResponse có)
        public string? CategoryName { get; set; } // ✅ Bổ sung CategoryName
        public string? PromotionTitle { get; set; } // ✅ Bổ sung PromotionTitle
        public List<string> Colors { get; set; } = new(); // ✅ Bổ sung Colors
    }

}
