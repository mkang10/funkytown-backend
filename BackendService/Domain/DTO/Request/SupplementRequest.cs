using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Request
{
    public class SupplementImportTransferRequestDto
    {
        public int OriginalImportId { get; set; }
        public List<SupplementImportDetailDto> ImportDetails { get; set; } = new();
    }

    public class SupplementImportTransferDetailDto
    {
        public int ProductVariantId { get; set; }
        public decimal UnitPrice { get; set; }
    }

}
