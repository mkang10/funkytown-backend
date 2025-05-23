using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.HelperServices.Models
{
    public class PayOSCreateResult
    {
        public string? CheckoutUrl { get; set; }
        public long OrderCode { get; set; }
    }
}
