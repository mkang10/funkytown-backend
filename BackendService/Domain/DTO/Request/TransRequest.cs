using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Request
{
    // DTO tạo mới giao dịch xuất hàng
    public class CreateInventoryTransactionDto
    {
        public int StoreId { get; set; }
        public string TransactionType { get; set; } = null!; // ví dụ: "Xuất bán", "Chuyển kho",...
        public int CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? Status { get; set; }
        public string? ReferenceNumber { get; set; }
        public decimal? TransactionCost { get; set; }

        // Danh sách chi tiết giao dịch
        public List<CreateInventoryTransactionDetailDto> Details { get; set; } = new List<CreateInventoryTransactionDetailDto>();

        // Danh sách lịch sử giao dịch
        public List<CreateInventoryTransactionHistoryDto> Histories { get; set; } = new List<CreateInventoryTransactionHistoryDto>();

        // Danh sách tài liệu (nếu cần)
        public List<CreateDocumentDto> Documents { get; set; } = new List<CreateDocumentDto>();
    }

    public class CreateInventoryTransactionDetailDto
    {
        public int ProductVariantId { get; set; }
        public int Quantity { get; set; }
    }

    public class CreateInventoryTransactionHistoryDto
    {
        public string Status { get; set; } = null!;
        public int ChangedBy { get; set; }
        public DateTime? ChangedDate { get; set; }
        public string? Comments { get; set; }
    }

    public class CreateDocumentDto
    {
        public int UploadedBy { get; set; }
        public DateTime? UploadedDate { get; set; }
        public string? FilePath { get; set; }
        public string? ImagePath { get; set; }
    }

}