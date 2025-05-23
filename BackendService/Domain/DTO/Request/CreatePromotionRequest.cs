using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Request
{
    public class CreatePromotionRequest
    {
        [Required]
        [MaxLength(255)]
        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        [Required]
        [RegularExpression("PERCENTAGE|FIXED_AMOUNT|FREE_SHIPPING", ErrorMessage = "Invalid DiscountType")]
        public string DiscountType { get; set; } = null!; // "PERCENTAGE" | "FIXED_AMOUNT" | "FREE_SHIPPING"

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "DiscountValue must be greater than zero")]
        public decimal DiscountValue { get; set; } // Giá trị giảm

        [Range(0, double.MaxValue)]
        public decimal? MinOrderAmount { get; set; } // Giá trị đơn hàng tối thiểu để áp dụng

        [Range(0, double.MaxValue)]
        public decimal? MaxDiscountAmount { get; set; } // Giới hạn giảm giá tối đa (nếu DiscountType = PERCENTAGE)

        [Required]
        [RegularExpression("PRODUCT|CATEGORY|ORDER", ErrorMessage = "Invalid ApplyTo")]
        public string ApplyTo { get; set; } = null!; // "PRODUCT", "CATEGORY", "ORDER"

        public List<int>? ApplyValue { get; set; } // Danh sách ID sản phẩm hoặc danh mục áp dụng

        [Required]
        public DateTime StartDate { get; set; } // Ngày bắt đầu

        [Required]
        public DateTime EndDate { get; set; } // Ngày kết thúc

        [Required]
        [RegularExpression("ACTIVE|INACTIVE", ErrorMessage = "Invalid Status")]
        public string Status { get; set; } = "ACTIVE"; // Trạng thái khuyến mãi
    }

}
