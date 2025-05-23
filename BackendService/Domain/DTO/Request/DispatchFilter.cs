using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Request
{
    public class DispatchFilterDto
    {
        public string? Status { get; set; }
        public string? ReferenceNumber { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? WarehouseId { get; set; }
        public int? StaffDetailId { get; set; }

        public string? SortBy { get; set; }
        public bool IsDescending { get; set; } = false;

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }


}


