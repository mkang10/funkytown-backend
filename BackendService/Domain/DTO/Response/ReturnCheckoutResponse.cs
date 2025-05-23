using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Response
{
    public class ReturnCheckoutResponse
    {
        public string ReturnCheckoutSessionId { get; set; } = string.Empty; // ✅ ID của phiên đổi trả
        public int OrderId { get; set; }
        public int AccountId { get; set; }
        public List<ReturnItemResponse> ReturnItems { get; set; } = new(); // ✅ Chỉ chứa thông tin sản phẩm
        public decimal TotalRefundAmount { get; set; } // ✅ Tổng tiền hoàn trả nếu đủ điều kiện
        public List<string> RefundMethods { get; set; } = new(); // ✅ Danh sách phương thức hoàn tiền
        public List<string> ReturnReasons { get; set; } = new(); // ✅ Lý do đổi trả để khách chọn
        public List<string> ReturnOptions { get; set; } = new(); // ✅ Phương án đổi trả (Đổi hàng / Hoàn tiền)
        public string ReturnDescription { get; set; } = string.Empty; // ✅ Mô tả chi tiết lý do đổi trả
        public List<string> MediaUrls { get; set; } = new(); // ✅ Danh sách hình ảnh/video minh chứng
        public string Email {  get; set; } = string.Empty;
    }


}
