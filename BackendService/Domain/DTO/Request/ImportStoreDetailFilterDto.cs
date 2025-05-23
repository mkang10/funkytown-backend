using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Request
{
    public class ImportStoreDetailFilterDto
    {
        // Lọc theo StaffDetailId (bắt buộc)
        public int StaffDetailId { get; set; }

        // Lọc theo Status (tùy chọn)
        public string? Status { get; set; }

        // Sắp xếp: mặc định sắp xếp theo ImportStoreId
        public string? SortBy { get; set; } = "ImportStoreId";
        public bool IsDescending { get; set; } = false;

        // Phân trang
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

}
