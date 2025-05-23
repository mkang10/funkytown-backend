using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Response
{
    public class PromotionListResponse
    {
        public int PromotionId { get; set; }
        public string Title { get; set; } = null!;
        public string DiscountType { get; set; } = null!;
        public decimal DiscountValue { get; set; }
        public string ApplyTo { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = null!;
    }

}
