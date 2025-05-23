using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Request
{
    public class StoreExportStoreDetailReq
    {
        public class StoreExportStoreDetailFilterDto
        {
            public int? DispatchDetailId { get; set; }
            public int? WarehouseId { get; set; }
            public int? StaffDetailId { get; set; }
            public int? HandleBy { get; set; }
            public string? Status { get; set; }
            public string? Comments { get; set; }
            public string? SortBy { get; set; }
            public bool IsDescending { get; set; } = false;
            public int Page { get; set; } = 1;
            public int PageSize { get; set; } = 10;
        }
    }
}
