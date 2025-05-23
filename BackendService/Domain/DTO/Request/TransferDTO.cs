using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Request
{
    // CreateTransferFullFlowDto.cs
    public class CreateTransferFullFlowDto
    {
        public int CreatedBy { get; set; }
        public int SourceWarehouseId { get; set; }
        public int DestinationWarehouseId { get; set; }


        // Tùy chọn nếu bạn muốn cho phép người dùng nhập sẵn ReferenceNumber
        public string? DispatchReferenceNumber { get; set; }
        public string? ImportReferenceNumber { get; set; }

        // Danh sách các chi tiết chuyển hàng
        public List<CreateTransferDetailDto> TransferDetails { get; set; } = new List<CreateTransferDetailDto>();
    }

    // CreateTransferDetailDto.cs
    public class CreateTransferDetailDto
    {
        public int VariantId { get; set; }
        public int Quantity { get; set; }
        public decimal CostPrice { get; set; } // Dùng để tính tổng chi phí cho Import
    }

    // TransferFullFlowDto.cs
    public class TransferFullFlowDto
    {
        public int TransferOrderId { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Status { get; set; }
        public List<TransferDetailDto> TransferDetails { get; set; } = new List<TransferDetailDto>();

        // Thông tin liên quan Import và Dispatch nếu cần trả về
        public int ImportId { get; set; }
        public int DispatchId { get; set; }
    }

    // TransferDetailDto.cs
    public class TransferDetailDto
    {
        public int TransferOrderDetailId { get; set; }
        public int VariantId { get; set; }
        public int Quantity { get; set; }
        public int? DeliveredQuantity { get; set; }


    }

}
