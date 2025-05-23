using Domain.DTO.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Response
{
    public class CheckOutData
    {
        public int AccountId { get; set; }
        public decimal SubTotal { get; set; }
        public decimal ShippingCost { get; set; }
        public int? ShippingAddressId { get; set; }
        public int? WarehouseId { get; set; } 
        public List<OrderItemRequest> Items { get; set; } = new();
    }
}
