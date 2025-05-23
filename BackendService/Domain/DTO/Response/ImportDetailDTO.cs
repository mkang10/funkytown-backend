using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Response
{

    public class InventoryImportDetailDto
    {
        public int ImportId { get; set; }
        public string? ReferenceNumber { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? Status { get; set; }
        public decimal? TotalCost { get; set; }
        public string CreatedByName { get; set; }  // Lấy từ Account.Name

        public DateTime? ApprovedDate { get; set; }

        public DateTime? CompletedDate { get; set; }
        public List<InventoryImportDetailItemDto> Details { get; set; } = new();
        public List<AuditLogRes> AuditLogs { get; set; } = new List<AuditLogRes>();

    }

    public class InventoryImportDetailItemDto
    {
        public int ImportDetailId { get; set; }
        public int ProductVariantId { get; set; }
        public int Quantity { get; set; }
        public int CostPrice { get; set; }

        public string ProductVariantName { get; set; }  // Lấy từ ProductVariant.Name

        public List<InventoryImportStoreDetailDto> StoreDetails { get; set; } = new();
    }

    public class InventoryImportStoreDetailDto
    {
        public int StoreId { get; set; }
        public string StoreName { get; set; }  // Lấy từ Store.Name
        public int AllocatedQuantity { get; set; }
        public int ActualQuantity { get; set; }

        public string? Status { get; set; }
        public string? Comments { get; set; }
        public int? StaffDetailId { get; set; }
        public string? StaffName { get; set; }  // Lấy từ StaffDetail.Name
    }

    
}
