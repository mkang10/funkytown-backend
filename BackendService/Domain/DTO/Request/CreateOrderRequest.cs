using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Request
{
    public class CreateOrderRequest
    {
        public int AccountId { get; set; }
        public string CheckOutSessionId { get; set; } = null!;
        public int? ShippingAddressId { get; set; }
        public string PaymentMethod { get; set; } = null!;
    }
}
