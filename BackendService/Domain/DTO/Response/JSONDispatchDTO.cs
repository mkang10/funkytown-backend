using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Response
{
    public class JSONDispatchDTO
    {
        public int DispatchId { get; set; }

        public string CreatedByUser { get; set; }

        public DateTime CreatedDate { get; set; }

        public string Status { get; set; } = null!;

        public string? ReferenceNumber { get; set; }

        public string? Remarks { get; set; }

        public int? OriginalId { get; set; }

        public DateTime? CompletedDate { get; set; }

        public List<JSONDispatchDetailDTO> Details { get; set; } = new();


    }
   
    public class JSONDispatchDetailDTO
    {
        public int DispatchDetailId { get; set; }

        public int DispatchId { get; set; }

        public string VariantName { get; set; }
        public string SizeName { get; set; }

        public string ColorName { get; set; }


        public int Quantity { get; set; }
        public decimal PriceProductVariant { get; set; }

        public List<JSONStoreExportDetailDTO> StoreExportDetail { get; set; } = new();

    }

    public class JSONStoreExportDetailDTO
    {
        public string WarehouseName { get; set; }

        public int AllocatedQuantity { get; set; }
        

        public string? Status { get; set; }

        public string? Comments { get; set; }

        public string? Staff { get; set; }

        public int? DispatchDetailId { get; set; }

        public string? HandleBy { get; set; }

        public int DispatchStoreDetailId { get; set; }

        public int? ActualQuantity { get; set; }

    }
    
 public class DispatchGet
    {
        public JSONDispatchDTO JSONDispatchGet { get; set; } = new();
        public List<AuditLogRes> AuditLogs { get; set; } = new List<AuditLogRes>();

    }
        public class JSONStoreExportStoreDetailByIdHandlerDTO
    {
        public string WarehouseName { get; set; }

        public string WarehouseDestinationName { get; set; }
        public string ProductName { get; set; }
        public string SizeName { get; set; }

        public string ColorName { get; set; }


        public int AllocatedQuantity { get; set; }

        public string? Status { get; set; }

        public string? Comments { get; set; }

        public string? Staff { get; set; }

        public int? DispatchDetailId { get; set; }

        public string? HandleBy { get; set; }

        public int DispatchStoreDetailId { get; set; }

        public int? ActualQuantity { get; set; }
        public string? ReferenceNumber { get; set; }
       
        public List<AuditLogRes> AuditLogs { get; set; } = new List<AuditLogRes>();


    }
}
