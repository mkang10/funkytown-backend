using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Request
{
    // DTO dùng cho tạo Product, nhận trực tiếp file upload
    // DTO cho Product image, sử dụng sau khi upload
    public class ProductImageDto
    {
        public string ImagePath { get; set; } = null!;
        public bool IsMain { get; set; }
    }

    // DTO request tạo Product, nhận file upload
    public class ProductCreateDto
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int? CategoryId { get; set; }
        public string? Origin { get; set; }
        public string? Model { get; set; }
        public string? Occasion { get; set; }
        public string? Style { get; set; }
        public string? Material { get; set; }
        public string? Status { get; set; }

        // Nhận trực tiếp các file ảnh từ client
        public List<IFormFile> Images { get; set; } = new();
    }

    public class ProductVariantCreateDto
    {
        public int MaxStocks { get; set; }
        public int ProductId { get; set; }
        public int? SizeId { get; set; }
        public int? ColorId { get; set; }
        public IFormFile? ImageFile { get; set; }
        public decimal? Weight { get; set; }


    }

}
