using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Response
{
    public class RevenueByDateResponse
    {
        public string TimePeriod { get; set; } = null!; // ngày / tháng / năm tùy theo kiểu
        public decimal TotalRevenue { get; set; }
    }

}
