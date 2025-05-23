using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Request
{
    public class StoreExportStoreDetailFilterDtO
    {
        public int? StaffDetailId { get; set; }
        public string? Status { get; set; }
        public string? SortBy { get; set; }
        public bool IsDescending { get; set; } = true;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
