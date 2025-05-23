using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Request
{
    public class ChangeCartItemQuantityRequest
    {
        public int ProductVariantId { get; set; }
        public int QuantityChange { get; set; } // +1 hoặc -1
    }

}
