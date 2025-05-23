using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Request
{
    public class UpdateReturnOrderStatusRequest
    {
        public int ReturnOrderId { get; set; }
        public string NewStatus { get; set; } = null!;
        public string? Comment { get; set; }
    }

}
