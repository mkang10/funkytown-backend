using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Request
{
    public class SubmitReturnRequest
    {
        public string ReturnCheckoutSessionId { get; set; } = string.Empty;
        public string? Email { get; set; } = string.Empty;
        public string ReturnReason { get; set; } = string.Empty;
        public string ReturnOption { get; set; } = string.Empty;
        public string RefundMethod { get; set; } = string.Empty;
        public string ReturnDescription { get; set; } = string.Empty;
        public List<IFormFile> MediaFiles { get; set; } = new();
        public string? BankName { get; set; }
        public string? BankAccountNumber { get; set; }
        public string? BankAccountName { get; set; }
    }

}
