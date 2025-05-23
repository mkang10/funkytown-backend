using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Response
{
    public class TransferResponseDto
    {
        public int TransferOrderId { get; set; }
        public int ImportId { get; set; }
        public string ImportReferenceNumber { get; set; } = null!;
        public int DispatchId { get; set; }
        public string DispatchReferenceNumber { get; set; } = null!;
        public string CreatedByName { get; set; }

        public int CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string Status { get; set; } = null!;
        public string? Remarks { get; set; }
        public int? OriginalTransferOrderId { get; set; }
    }

    public class TransferDto
    {
        public int TransferOrderId { get; set; }
        public int ImportId { get; set; }
        public string ImportReferenceNumber { get; set; } = null!;
        public int DispatchId { get; set; }
        public string DispatchReferenceNumber { get; set; } = null!;
        public string CreatedByName { get; set; }

        public int CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string Status { get; set; } = null!;
        public string? Remarks { get; set; }
        public int? OriginalTransferOrderId { get; set; }
    }
}
