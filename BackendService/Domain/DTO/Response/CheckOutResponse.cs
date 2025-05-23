using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Response
{
    public class CheckOutResponse
    {
        public string CheckOutSessionId { get; set; } = null!;
        public decimal SubTotal { get; set; }
        public decimal ShippingCost { get; set; }
    
        public List<string> AvailablePaymentMethods { get; set; } = new();
        public List<ShippingAddress> ShippingAddresses { get; set; }
        public ShippingAddress ShippingAddress { get; set; } = null!;
        public List<OrderItemResponse> Items { get; set; } = new();
    }
}
