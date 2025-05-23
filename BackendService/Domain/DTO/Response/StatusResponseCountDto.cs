using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Response
{
    public class StatusCountDto
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class DashboardDto
    {
        public List<StatusCountDto> ImportStatusCounts { get; set; } = new();
        public List<StatusCountDto> DispatchStatusCounts { get; set; } = new();
        public List<StatusCountDto> TransferStatusCounts { get; set; } = new();

        // Summary fields
        public int TotalImports { get; set; }
        public decimal TotalImportCost { get; set; }
        public int TotalDispatches { get; set; }
        public int TotalTransfers { get; set; }
    }
}
