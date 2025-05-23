using Microsoft.AspNetCore.Http;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Request
{
    public class EditProfileRequest
    {
        [Required]
        public string FullName { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        [SwaggerSchema("string", Format = "binary")]
        public IFormFile? AvatarImage { get; set; }
        [SwaggerSchema("string", Format = "date")]
        public DateOnly? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? PreferredPaymentMethod { get; set; }
    }
}
