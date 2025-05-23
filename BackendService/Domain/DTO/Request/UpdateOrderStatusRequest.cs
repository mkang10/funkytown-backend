using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Request
{
    public class UpdateOrderStatusRequest
    {
        public string NewStatus { get; set; } = null!;
        public int ChangedBy { get; set; }
        public string? Comment { get; set; }
    }
}
