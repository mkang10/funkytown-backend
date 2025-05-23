using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Response
{
    public class OrderDoneRes

    {
        public class OrderResponseDTO
        {
            public int OrderId { get; set; }
            public string? Status { get; set; }
        }
    }
}
