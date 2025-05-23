using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Request
{
    public class SupplementImportRequestDto
    {
        public int OriginalImportId { get; set; }
        public List<SupplementImportDetailDto> ImportDetails { get; set; } = new();
    }

    public class SupplementImportDetailDto
    {
        public int ProductVariantId { get; set; }
        public decimal CostPrice { get; set; }
    }
}
