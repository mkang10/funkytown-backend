using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Response
{
    public class ProductVariantResponse
    {
        public int VariantId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public string? Size { get; set; }
        public string? Color { get; set; }
        public decimal Price { get; set; }
        public decimal DiscountedPrice { get; set; } // Giá sau khi áp dụng khuyến mãi
        public string? PromotionTitle { get; set; } // Tên khuyến mãi nếu có
        public int? StockQuantity { get; set; }
        public string? ImagePath { get; set; }
        public string? Sku { get; set; }
        public string? Barcode { get; set; }
        public decimal? Weight { get; set; }
    }

    public class ProductVariantResponseDto
    {
        public int VariantId { get; set; }
        public string ProductName { get; set; } = null!;
        public string? SizeName { get; set; }
        public string? ColorName { get; set; }
        public string? MainImagePath { get; set; }
    }

    public class ProductVariantResponseOrder
    {
        public int VariantId { get; set; } // ID của biến thể sản phẩm
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty; // Tên sản phẩm
        public string? Size { get; set; } // Kích thước (có thể null)
        public string? Color { get; set; } // Màu sắc (có thể null)
        public decimal Price { get; set; } // Giá sản phẩm
        public decimal DiscountedPrice { get; set; } // Giá sau khi áp dụng khuyến mãi
        public string? PromotionTitle { get; set; } // Tên khuyến mãi nếu có
        public int? StockQuantity { get; set; } // Số lượng tồn kho
        public string? ImagePath { get; set; } // Ảnh đại diện sản phẩm
        public string? Sku { get; set; } // Mã SKU
        public string? Barcode { get; set; } // Mã vạch sản phẩm
        public decimal? Weight { get; set; } // Cân nặng sản phẩm
        public List<string> AdditionalImages { get; set; } = new(); // Danh sách ảnh bổ sung
        public string? Description { get; set; } // Mô tả sản phẩm
    }

}
