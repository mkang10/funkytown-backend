using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Response
{
    public class JSONImportDTO
    {
        public int ImportId { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; }

        public string? Status { get; set; }

        public string? ReferenceNumber { get; set; }

        public decimal? TotalCost { get; set; }

        public DateTime? ApprovedDate { get; set; }

        public DateTime? CompletedDate { get; set; }

        public int? OriginalImportId { get; set; }
        public List<JSONImportDetailDTO> Details { get; set; } = new();

    }
    public class JSONImportDetailDTO
    {
        public int ImportDetailId { get; set; }

        public int ImportId { get; set; }

        public string Product { get; set; }
        public string Size { get; set; }
        public string Color { get; set; }


        public int Quantity { get; set; }
        public decimal? CostPrice { get; set; }
        public decimal PriceProductVariant { get; set; }


        public List<JSONImportStoreDetailGetDTO> StoreImportDetail { get; set; } = new();

    }
    public class JSONImportStoreDetailGetDTO
    {
        public int? ActualReceivedQuantity { get; set; }

        public int AllocatedQuantity { get; set; }

        public string? Status { get; set; }

        public string? Comments { get; set; }

        public string? Staff { get; set; }

        public int ImportDetailId { get; set; }

        public int ImportStoreId { get; set; }

        public string? WarehouseName { get; set; }

        public string? HandleBy { get; set; }

    }

    public class ImportGet
    {
        public JSONImportDTO JSONImportGet { get; set; } = new();

        public List<AuditLogRes> AuditLogs { get; set; } = new List<AuditLogRes>();

    }

    public class JSONImportStoreDetailDTO
    {
        public int? ActualReceivedQuantity { get; set; }

        public int AllocatedQuantity { get; set; }

        public string? Status { get; set; }

        public string? Comments { get; set; }

        public string? Staff { get; set; }

        public int ImportDetailId { get; set; }

        public int ImportStoreId { get; set; }

        public string? WarehouseName { get; set; }
        public string? ProductName { get; set; }
        public string? SizeName { get; set; }
        public string? ColorName { get; set; }


        public string? HandleBy { get; set; }

        public string? ReferenceNumber { get; set; }
        public decimal? CostPrice { get; set; }


        public List<AuditLogRes> AuditLogs { get; set; } = new List<AuditLogRes>();

    }
}
