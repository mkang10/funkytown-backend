using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Request
{
    public class ApproveRejectRequestDto
    {
        public int ChangedBy { get; set; }
        public string? Comments { get; set; }
    }
}
