using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Request
{
    public class UpdateShippingAddressRequest
    {
        [Required(ErrorMessage = "Địa chỉ không được để trống")]
        [StringLength(255, ErrorMessage = "Địa chỉ không được vượt quá 255 ký tự")]
        public string Address { get; set; } = null!;

        public string City { get; set; } = null!;

        public string Province { get; set; } = null!;

        public string District { get; set; } = null!;

        public string Country { get; set; } = null!;

        [Required(ErrorMessage = "Tên người nhận không được để trống")]
        [StringLength(100, ErrorMessage = "Tên người nhận không được vượt quá 100 ký tự")]
        public string RecipientName { get; set; } = null!;

        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [StringLength(15, MinimumLength = 9, ErrorMessage = "Số điện thoại phải từ 9 đến 15 số")]
        public string RecipientPhone { get; set; } = null!;

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự")]
        public string Email { get; set; } = null!;

        public bool IsDefault { get; set; }
    }
}
