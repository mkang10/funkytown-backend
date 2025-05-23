using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Response
{
    public class OrderAssigmentRes
    {
        public class OrderAssignmentResponseDTO
        {
            public int AssignmentId { get; set; }
            public int OrderId { get; set; }
            public int? StaffId { get; set; }
            public DateTime AssignmentDate { get; set; }
            public string? Comments { get; set; }
        }
    }
}
