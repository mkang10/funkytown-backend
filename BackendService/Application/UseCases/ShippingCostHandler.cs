using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class ShippingCostHandler
    {
        private readonly IConfiguration _configuration;

        public ShippingCostHandler(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Tính phí vận chuyển dựa trên thành phố và quận/huyện.
        /// Nếu city là "Hồ Chí Minh" thì tính phí nội thành, còn lại tính phí ngoại thành.
        /// </summary>
        /// <param name="city">Tên thành phố</param>
        /// <param name="district">Tên quận/huyện (có thể dùng để mở rộng logic sau này)</param>
        /// <returns>Phí vận chuyển (decimal)</returns>
        public decimal CalculateShippingCost(string city, string district)
        {
            // Đọc phí từ cấu hình (hoặc sử dụng giá trị mặc định nếu không parse được)
            decimal urbanFee = decimal.TryParse(_configuration["ShippingFees:Urban"], out var uFee) ? uFee : 20000;
            decimal suburbanFee = decimal.TryParse(_configuration["ShippingFees:Suburban"], out var sFee) ? sFee : 35000;

            // Nếu city là "Hồ Chí Minh" (không phân biệt chữ hoa thường), tính phí nội thành, ngược lại ngoại thành.
            return city?.Trim().Equals("Hồ Chí Minh", StringComparison.OrdinalIgnoreCase) == true
                ? urbanFee
                : suburbanFee;
        }
    }
}
