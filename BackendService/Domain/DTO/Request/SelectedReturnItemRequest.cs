using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Request
{
    public class SelectedReturnItemRequest
    {
        public int ProductVariantId { get; set; } // ✅ ID của biến thể sản phẩm
        public int Quantity { get; set; } // ✅ Số lượng khách muốn trả
    }

}
