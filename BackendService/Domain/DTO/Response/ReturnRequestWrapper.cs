using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Response
{
    public class ReturnRequestWrapper
    {
        public ReturnOrderInfo ReturnOrder { get; set; } = new();
        public ReturnOrderDetailInfo ReturnOrderDetail { get; set; } = new();
        public List<ReturnOrderItemInfo> ReturnOrderItems { get; set; } = new();
    }
    public class ReturnOrderInfo
    {
        public int ReturnOrderId { get; set; }
        public int OrderId { get; set; }
        public string AccountName { get; set; } = string.Empty;
        public decimal TotalRefundAmount { get; set; }
        public string ReturnOption { get; set; } = string.Empty;
        public string ReturnDescription { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
    }
    public class ReturnOrderDetailInfo
    {
        public DateTime? UpdatedDate { get; set; } // Nếu chưa có thì bạn phải thêm field này vào entity
        public string? BankName { get; set; }
        public string? BankAccountNumber { get; set; }
        public string? BankAccountName { get; set; }
        public string? RefundMethod { get; set; }
        public List<string>? ReturnImages { get; set; }
    }
    public class ReturnOrderItemInfo
    {
        public string ProductVariantName { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal PriceAtPurchase { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal Price { get; set; }
    }

}
