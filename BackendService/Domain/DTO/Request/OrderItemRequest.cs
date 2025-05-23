using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Request

{
    public class OrderItemRequest
    {
        public int ProductVariantId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal DiscountedPrice { get; set; } // Giá sau khi áp dụng khuyến mãi
        public string? PromotionTitle { get; set; } // Tên khuyến mãi nếu có
        public string ProductName { get; set; } // Tên sản phẩm
        public string ImageUrl { get; set; } // Ảnh sản phẩm
        public string Size { get; set; } // Kích thước của variant
        public string Color { get; set; } // Màu sắc của variant
    }
}
