using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Response
{
    public class CreatePaymentResponse
    {
        public string? CheckoutUrl { get; set; }
        public long OrderCode { get; set; }
    }

}
