using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Request
{
    public class OrderDetailRequest
    {
        public string order_code { get; set; }
    }
    public class OrderDetailWithUpdateRequest
    {
        public string order_code { get; set; }

    }
}   
