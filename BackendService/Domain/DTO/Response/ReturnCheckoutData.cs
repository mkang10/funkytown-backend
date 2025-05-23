using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Response
{
    public class ReturnCheckoutData
    {
        public int AccountId { get; set; }
        public int OrderId { get; set; }
        public decimal TotalRefundAmount { get; set; }
        public List<ReturnItemResponse> Items { get; set; } = new();
    }

}
