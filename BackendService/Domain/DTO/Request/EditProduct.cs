using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Request
{
    public class ProductImageUpdateDto
    {
        [Required]
        public int? ProductImageId { get; set; }

        /// <summary>
        /// Nếu user muốn thay file ảnh mới, truyền lên file;
        /// ngược lại để null để giữ nguyên đường dẫn cũ.
        /// </summary>
        public IFormFile? ImageFile { get; set; }

        /// <summary>
        /// Cập nhật lại flag IsMain (có thể true/false).
        /// </summary>
        public bool IsMain { get; set; }
    }

    // 2. DTO cho thêm ảnh mới
    public class ProductImageAddDto
    {
        [Required]
        public IFormFile ImageFile { get; set; } = null!;

        /// <summary>
        /// Đánh dấu ảnh mới có là ảnh chính hay không.
        /// </summary>
        public bool IsMain { get; set; }
    }

    // 3. DTO chính cho Edit Product, kết hợp
    public class ProductEditDto
    {
       

        [Required]
        public string Name { get; set; } = null!;

        public string? Description { get; set; }
        public int? CategoryId { get; set; }
        public string? Origin { get; set; }
        public string? Model { get; set; }
        public string? Occasion { get; set; }
        public string? Style { get; set; }
        public string? Material { get; set; }
        public string? Status { get; set; }

        /// <summary>
        /// Danh sách ảnh đã có, có thể cập nhật file mới và/or cập nhật IsMain.
        /// </summary>
        public List<ProductImageUpdateDto> ExistingImages { get; set; } = new List<ProductImageUpdateDto>();

        /// <summary>
        /// Danh sách ảnh mới sẽ được thêm vào.
        /// </summary>
        public List<ProductImageAddDto> NewImages { get; set; } = new List<ProductImageAddDto>();
    }

    // 4. DTO cho Edit Variant (nếu cần)
    public class ProductVariantEditDto
    {
        [Required]
        public int VariantId { get; set; }

        public int? SizeId { get; set; }
        public int? ColorId { get; set; }

        /// <summary>
        /// Nếu thay ảnh, truyền file; để null để giữ nguyên.
        /// </summary>
        public IFormFile? ImageFile { get; set; }

        public decimal? Weight { get; set; }
    }
}
