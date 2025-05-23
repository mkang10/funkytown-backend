using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Request
{
    public class ImportStoreDetailFilterDtO
    {
        public int HandleBy { get; set; } // Bắt buộc
        public string? Status { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; }
        public bool IsDescending { get; set; } = true;
    }
}
