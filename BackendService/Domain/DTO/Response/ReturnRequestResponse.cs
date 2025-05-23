using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Response
{
    public class ReturnRequestResponse
    {
        public int ReturnOrderId { get; set; }
        public int OrderId { get; set; }
        public string Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public decimal TotalRefundAmount { get; set; }
        public string RefundMethod { get; set; }
        public string ReturnReason { get; set; }
        public string ReturnOption { get; set; }
        public string ReturnDescription { get; set; }
        public List<string>? ReturnImages { get; set; }

        // Thông tin ngân hàng (nếu có)
        public string? BankName { get; set; }
        public string? BankAccountNumber { get; set; }
        public string? BankAccountName { get; set; }
        public List<ReturnItemResponse> ReturnItems { get; set; } = new();

        // Dùng trực tiếp wrapper trả về chi tiết order
        public OrderDetailResponseWrapper? Order { get; set; }
    }


}
