using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Request
{
    public class PayOSCallbackRoot
	{
		public string code { get; set; }
		public string desc { get; set; }
		public bool success { get; set; }
		public PayOSCallbackData data { get; set; }
		public string signature { get; set; }
	}

	public class PayOSCallbackData
	{
		public string accountNumber { get; set; }
		public decimal amount { get; set; }
		public string description { get; set; }
		public string reference { get; set; }
		public string transactionDateTime { get; set; }
		public string virtualAccountNumber { get; set; }
		public string counterAccountBankId { get; set; }
		public string counterAccountBankName { get; set; }
		public string? counterAccountName { get; set; }
		public string counterAccountNumber { get; set; }
		public string virtualAccountName { get; set; }
		public string currency { get; set; }
		public int orderCode { get; set; }
		public string paymentLinkId { get; set; }
		public string code { get; set; }
		public string desc { get; set; }
		// Thêm các trường khác nếu có
	}
}
