using System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Request
{
    public class TransImportDto
    {
        public int CreatedBy { get; set; }
        public int WarehouseId { get; set; }

        public bool IsUrgent { get; set; } = false;

        public List<TransImportDetailDto> ImportDetails { get; set; } = new();
    }

    public class TransImportDetailDto
    {
        public int ProductVariantId { get; set; }
        public int Quantity { get; set; }
    }
}
