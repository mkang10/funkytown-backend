using Domain.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Response
{
    
        public class GetWareHouseStockRes
        {
        public int WareHouseStockId { get; set; }
        public int VariantId { get; set; }
        public string ProductName { get; set; } = null!;
        public string VariantName { get; set; } = null!;
        public string? SizeName { get; set; }
        public string? ColorName { get; set; }
        public int StockQuantity { get; set; }
        public int WareHouseId { get; set; }
        public string WareHouseName { get; set; } = null!;
    }
    
}
