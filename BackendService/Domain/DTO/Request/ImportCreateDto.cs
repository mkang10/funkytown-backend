using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Request
{
    public class ImportCreateDto
    {
        public int CreatedBy { get; set; }
        public bool IsUrgent { get; set; }
        public List<ImportItemDto> Items { get; set; } = new();
    }

    public class ImportItemDto
    {
        public int ProductVariantId { get; set; }
        public int Quantity { get; set; }
        public decimal CostPrice { get; set; }
        public int? WarehouseId { get; set; }
    }

}
