using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Response
{
    public class StockQuantityResponse
    {
        public int WarehouseId { get; set; }
        public int ProductVariantId { get; set; }
        public int StockQuantity { get; set; }
    }
}
