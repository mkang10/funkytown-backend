using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Request
{
    public class UpdatePromotionRequest
    {
        [MaxLength(255)]
        public string? Title { get; set; }

        public string? Description { get; set; }

        [RegularExpression("PERCENTAGE|FIXED_AMOUNT|FREE_SHIPPING", ErrorMessage = "Invalid DiscountType")]
        public string? DiscountType { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal? DiscountValue { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? MinOrderAmount { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? MaxDiscountAmount { get; set; }

        [RegularExpression("PRODUCT|CATEGORY|ORDER", ErrorMessage = "Invalid ApplyTo")]
        public string? ApplyTo { get; set; }

        public List<int>? ApplyValue { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [RegularExpression("ACTIVE|INACTIVE", ErrorMessage = "Invalid Status")]
        public string? Status { get; set; }
    }
}
