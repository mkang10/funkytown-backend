using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Response
{
    public class OrderSummaryItem
    {
        public int OrderId { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string CustomerName { get; set; } = null!;
        public int TotalQuantity { get; set; }
        public decimal TotalPrice { get; set; } // Tổng sau giảm
    }
}
