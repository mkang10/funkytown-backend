using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Request
{
    public class CheckOutRequest
    {
        public int AccountId { get; set; }
        public List<int> SelectedProductVariantIds { get; set; } = new();
        //public int? ShippingAddressId { get; set; } // Địa chỉ giao hàng, mặc định nếu không có
        //public int? StoreId { get; set; } // Nếu người dùng muốn chọn cửa hàng
        //public string? PaymentMethod { get; set; } // Nếu muốn chọn phương thức thanh toán ngay
    }
}
