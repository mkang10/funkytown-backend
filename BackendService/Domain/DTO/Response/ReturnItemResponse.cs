using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Response
{
    public class ReturnItemResponse
    {
        public int ProductVariantId { get; set; }
        public string ProductName { get; set; }
        public string Color { get; set; }
        public string Size { get; set; }
        public string ImageUrl { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
