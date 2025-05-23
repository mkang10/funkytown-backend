using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Request
{
    public class UpdateAssignmentSettingRequest
    {
        public int ShopManagerId { get; set; }
        public int StaffId { get; set; }
    }
}
