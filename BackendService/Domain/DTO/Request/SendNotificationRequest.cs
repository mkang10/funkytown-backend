using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Request
{
    public class SendNotificationRequest
    {
        public int AccountId { get; set; } // ID người nhận thông báo
        public string Title { get; set; } = string.Empty; // Tiêu đề thông báo
        public string Message { get; set; } = string.Empty; // Nội dung thông báo
        public string NotificationType { get; set; } = string.Empty; // Loại thông báo (e.g., "Order", "Payment")
        public int TargetId { get; set; } // ID đối tượng liên quan (e.g., OrderId, ProductId)
        public string TargetType { get; set; } = string.Empty; // Loại đối tượng (e.g., "Order", "Product")
    }


}
