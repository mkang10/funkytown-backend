using Domain.DTO.Response;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Template
{
    public class EmailTemplateBuilder
    {
        public static string BuildForgotPasswordEmail(string newPassword)
        {
            return $@"
                <html>
                <body style='font-family: Arial, sans-serif; color: #000;'>
                    <div style='text-align: center;'>
                        <img src='https://res.cloudinary.com/dpbscvwv3/image/upload/v1744794172/71ecf1849f9887a5649c505595aa7586_tn_uxgitg.webp' style='width: 60px; height: 60px; border-radius: 50%; object-fit: cover;' alt='Logo'/>
                        <h2 style='margin: 10px 0;'>FUNKY TOWN</h2>
                        <p style='margin: 0;'>Địa chỉ: 26 Lý Tự Trọng, Bến Nghé, Quận 1, Hồ Chí Minh 700000</p>
                        <p style='margin: 0;'>SĐT: 093.990.5767</p>
                        <p style='margin: 0;'>Shopee: shope.ee/9pFQssAyaF</p>
                        <p style='margin: 0;'>Instagram: https://www.instagram.com/funkytown.gallery/</p>
                    </div>

                    <div style='margin-top: 30px;'>
                        <p>Xin chào,</p>
                        <p>Chúng mình là đội ngũ Chăm sóc khách hàng từ <strong>FUNKY TOWN</strong>.</p>
                        <p>Chúng mình xin gửi bạn mật khẩu đăng nhập mới:</p>
                        <p style='font-size: 18px; font-weight: bold;'>Mật khẩu: {newPassword}</p>
                        <p>Chúc bạn một ngày mới tốt lành!</p>
                    </div>
                </body>
                </html>
            ";
        }
        public static string BuildInvoiceHtml(InvoiceForEmailDTO invoice)
        {
            var sb = new StringBuilder();

            sb.AppendLine("<html><body style='font-family: Arial, sans-serif; color: #000;'>");

            // Left-aligned Logo and Info
            sb.AppendLine("<div style='display: flex; justify-content: flex-start;'>");
            sb.AppendLine("<div>");
            sb.AppendLine("<img src='https://res.cloudinary.com/dpbscvwv3/image/upload/v1744794172/71ecf1849f9887a5649c505595aa7586_tn_uxgitg.webp' style='width: 60px; height: 60px; border-radius: 50%; object-fit: cover;' alt='Logo'/>");
            sb.AppendLine("<h2 style='margin: 0;'>FUNKY TOWN</h2>");
            sb.AppendLine("<p style='margin: 0;'>Địa chỉ: 26 Lý Tự Trọng, Bến Nghé, Quận 1, Hồ Chí Minh 700000<br/>SĐT: 093.990.5767<br/>Shopee: shope.ee/9pFQssAyaF<br/>Instagram: https://www.instagram.com/funkytown.gallery/</p>");
            sb.AppendLine("</div>");
            sb.AppendLine("</div>");

            sb.AppendLine("<hr style='margin: 20px 0;' />");

            sb.AppendLine($"<p><strong>Thời gian:</strong> {DateTime.Now:MM/dd/yyyy}</p>");

            // Customer Info
            sb.AppendLine("<div style='display: flex; justify-content: space-between;'>");
            sb.AppendLine("<div>");
            sb.AppendLine("<h3>Gửi đến:</h3>");
            sb.AppendLine($"<p>Tên: {invoice.FullName}<br/>Địa chỉ: {invoice.Address}<br/>SĐT: {invoice.PhoneNumber}<br/>Email: {invoice.Email}</p>");
            sb.AppendLine("</div>");
            sb.AppendLine("</div>");

            // Product Table
            sb.AppendLine("<h3>Hóa đơn chi tiết</h3>");
            sb.AppendLine("<table border='1' cellspacing='0' cellpadding='8' style='width: 100%; border-collapse: collapse;'>");
            sb.AppendLine("<thead style='background: #000; color: #fff;'>");
            sb.AppendLine("<tr><th>Tên sản phẩm</th><th>Size</th><th>Màu sắc</th><th>S.Lượng</th><th>Giá</th><th>Tổng</th></tr>");
            sb.AppendLine("</thead>");
            sb.AppendLine("<tbody>");

            decimal subTotal = 0;
            var culture = new CultureInfo("vi-VN");

            foreach (var item in invoice.OrderdetailEmail)
            {
                var p = item.Item;
                var itemTotal = item.Quantity * item.PriceAtPurchase;
                subTotal += itemTotal;

                sb.AppendLine("<tr>");
                sb.AppendLine($"<td>{p.ProductId}</td>");
                sb.AppendLine($"<td>{p.SizeId}</td>");
                sb.AppendLine($"<td>{p.ColorId}</td>");
                sb.AppendLine($"<td>{item.Quantity}</td>");
                sb.AppendLine($"<td>{string.Format(culture, "{0:#,##0} \u20ab", item.PriceAtPurchase)}</td>");
                sb.AppendLine($"<td>{string.Format(culture, "{0:#,##0} \u20ab", itemTotal)}</td>");
                sb.AppendLine("</tr>");
            }

            sb.AppendLine("</tbody>");
            sb.AppendLine("</table>");

            // Totals Section (aligned right)
            sb.AppendLine("<div style='margin-top: 20px;'>");
            sb.AppendLine("<div style='display: flex; justify-content: space-between;'>");

            var total = subTotal + invoice.ShippingCost;

            // Totals Section (đã sửa lại)
            sb.AppendLine("<div style='margin-top: 30px; border: 1px solid #ccc; width: 400px; padding: 15px;'>");
            sb.AppendLine("<table style='width: 100%; font-size: 16px;'>");

            sb.AppendLine("<tr>");
            sb.AppendLine("<td style='text-align: left;'>Tạm tính:</td>");
            sb.AppendLine($"<td style='text-align: right;'>{string.Format(culture, "{0:#,##0} \u20ab", subTotal)}</td>");
            sb.AppendLine("</tr>");

            sb.AppendLine("<tr>");
            sb.AppendLine("<td style='text-align: left;'>Phí vận chuyển:</td>");
            sb.AppendLine($"<td style='text-align: right;'>{string.Format(culture, "{0:#,##0} \u20ab", invoice.ShippingCost)}</td>");
            sb.AppendLine("</tr>");

            sb.AppendLine("<tr style='font-weight: bold;'>");
            sb.AppendLine("<td style='text-align: left;'>Tổng cộng:</td>");
            sb.AppendLine($"<td style='text-align: right;'>{string.Format(culture, "{0:#,##0} \u20ab", total)}</td>");
            sb.AppendLine("</tr>");

            sb.AppendLine("</table>");

            // Highlight tổng tiền phải trả
            sb.AppendLine("<div style='margin-top: 20px;'>");
            sb.AppendLine("<div style='background: #000; color: #fff; padding: 10px 20px; text-align: center; font-weight: bold;'>");
            sb.AppendLine($"Tổng tiền phải trả: {string.Format(culture, "{0:#,##0} \u20ab", total)}");
            sb.AppendLine("</div>");
            sb.AppendLine("</div>");

            sb.AppendLine("</div>");



            sb.AppendLine("</body></html>");

            return sb.ToString();
        }
    }
}