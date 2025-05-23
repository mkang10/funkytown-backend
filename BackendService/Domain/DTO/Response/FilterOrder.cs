using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Response
{
    // Domain/DTOs/OrderAssignmentFilterDto.cs
    public class OrderAssignmentFilterDto
    {
        // OrderAssignment
        public int? AssignmentId { get; set; }
        public int? ShopManagerId { get; set; }
        public int? StaffId { get; set; }
        public DateTime? AssignmentDateFrom { get; set; }
        public DateTime? AssignmentDateTo { get; set; }
        public string? CommentsContains { get; set; }

        // Order
        public DateTime? OrderCreatedDateFrom { get; set; }
        public DateTime? OrderCreatedDateTo { get; set; }
        public string? OrderStatus { get; set; }
        public decimal? MinOrderTotal { get; set; }
        public decimal? MaxOrderTotal { get; set; }
        public string? FullNameContains { get; set; }
    }

}
