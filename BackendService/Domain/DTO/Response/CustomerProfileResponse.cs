using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Response
{
    public class CustomerProfileResponse
    {
        public int AccountId { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? ImagePath { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public bool? IsActive { get; set; }

        // CustomerDetail
        public int? LoyaltyPoints { get; set; }
        public string? MembershipLevel { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? CustomerType { get; set; }
        public string? PreferredPaymentMethod { get; set; }
    }
}
