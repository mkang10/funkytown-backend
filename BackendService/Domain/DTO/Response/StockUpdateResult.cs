using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Response
{
    public class StockUpdateResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public List<StockItemResponse> UpdatedItems { get; set; }
    }
}
