using Domain.DTO.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Request
{
    public class StockUpdateRequest
    {
        public int WarehouseId { get; set; }
        public List<StockItemResponseOrder> Items { get; set; } = new();
    }
}
