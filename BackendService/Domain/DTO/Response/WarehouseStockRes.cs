using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Response
{
    public class WarehouseStockDto
    {
        public int WareHouseStockId { get; set; }
        public int VariantId { get; set; }
        public int StockQuantity { get; set; }
        public int WareHouseId { get; set; }
        public string WarehouseName { get; set; } = null!;
        public string FullProductName { get; set; } = null!;  // ProductName + Color + Size
    }
}
