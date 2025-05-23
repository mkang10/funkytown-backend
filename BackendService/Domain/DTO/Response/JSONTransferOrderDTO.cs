using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Response
{
    public class JSONTransferOrderDTO
    {
        public int TransferOrderId { get; set; }

        public int ImportId { get; set; }

        public int DispatchId { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; }

        public string Status { get; set; } = null!;

        public string? Remarks { get; set; }

        public int? OriginalTransferOrderId { get; set; }

        
        public List<JSONTransferOrderDetailDTO> DetailsTransferOrder { get; set; } = new();

    }
    public class JSONTransferDispatchImportGet
    {
        public JSONTransferOrderDTO JSONTransfer { get; set; } = new();
        public JSONDispatchDTO JSONDispatch { get; set; } = new();
        public JSONImportDTO JSONImport { get; set; } = new();
        public List<AuditLogRes> AuditLogs { get; set; } = new List<AuditLogRes>();
    }

    public class JSONTransferOrderDetailDTO
    {
        public int TransferOrderDetailId { get; set; }

        public int TransferOrderId { get; set; }

        public string Product { get; set; }

        public string Color { get; set; }

        public string Size { get; set; }

        public int Quantity { get; set; }

        public int? DeliveredQuantity { get; set; }

    }

   
}
