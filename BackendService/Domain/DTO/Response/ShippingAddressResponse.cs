using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Response
{
    public class ShippingAddressResponse
    {
        public int AddressId { get; set; }
        public int AccountId { get; set; }
        public string Address { get; set; } = null!;
        public string City { get; set; } = null!;
        public string Province { get; set; } = null!;
        public string District { get; set; } = null!;
        public string Country { get; set; } = null!;
        public string RecipientName { get; set; } = null!;
        public string RecipientPhone { get; set; } = null!;
        public string? Email { get; set; }
        public bool IsDefault { get; set; }
    }

}
