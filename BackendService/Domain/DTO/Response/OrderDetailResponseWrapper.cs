using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Domain.DTO.Response
{
    public class OrderDetailResponseWrapper
    {
        public int OrderId { get; set; }

        // Thông tin giao hàng từ Order
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string Province { get; set; } = string.Empty;
        public string? Country { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public decimal OrderTotal { get; set; }
        public decimal ShippingCost { get; set; }
        public string Status { get; set; } = null!;          
        public DateTime? CreatedDate { get; set; }
        public string? Ghnid { get; set; }
        public bool? IsFeedback { get; set; }
        public DateTime? CompletedDate { get; set; }
        public List<OrderItemResponse> OrderItems { get; set; } = new();
    }

}
