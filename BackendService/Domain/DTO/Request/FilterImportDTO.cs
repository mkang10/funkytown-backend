using Domain.DTO.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Request
{
    public class InventoryImportFilterDto
    {
        public string? Status { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDateFrom { get; set; }
        public DateTime? CreatedDateTo { get; set; }
        public string? ReferenceNumber { get; set; }
        public decimal? TotalCostMin { get; set; }
        public decimal? TotalCostMax { get; set; }
        public DateTime? ApprovedDateFrom { get; set; }
        public DateTime? ApprovedDateTo { get; set; }
        public DateTime? CompletedDateFrom { get; set; }
        public DateTime? CompletedDateTo { get; set; }

        // Sử dụng enum cho trường sắp xếp
        public InventoryImportSortField SortField { get; set; } = InventoryImportSortField.ImportId;

        // Trạng thái sắp xếp: true nếu giảm dần, false nếu tăng dần
        public bool IsDescending { get; set; } = false;

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }


}
