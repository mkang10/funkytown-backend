using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Response
{
    public class NotificationResponse
    {
        public int NotificationID { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string NotificationType { get; set; }
        public bool? IsRead { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
}
