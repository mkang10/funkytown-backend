using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Response
{

    public class ProductDetailResponseInven
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }

        public string? ImagePath { get; set; }
        public List<string>? ImagePaths { get; set; } = new();

        public string? Origin { get; set; }
        public string? Model { get; set; }
        public string? Occasion { get; set; }
        public string? Style { get; set; }
        public string? Material { get; set; }
        public string? CategoryName { get; set; }
        public bool? IsFavorite { get; set; }
        public List<ProductVariantResponse> Variants { get; set; } = new();
    }
    public class ProductDetailResponse
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ImagePath { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string? Style { get; set; }
    }
    public class ProductListResponse
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string? ImagePath { get; set; }
        public decimal Price { get; set; } // Giá của sản phẩm
        public decimal DiscountedPrice { get; set; } // Giá sau khi áp dụng khuyến mãi
        public string? CategoryName { get; set; }
        public string? PromotionTitle { get; set; } // Tên khuyến mãi nếu có
        public bool? IsFavorite { get; set; }
        public List<string> Colors { get; set; } = new();
    }
    public class ColorInfo
    {
        public int ColorId { get; set; }
        public string ColorName { get; set; }
        public string? ColorCode { get; set; }
    }

}
