using Application.Template;
using Domain.DTO.Response;
using Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class AuthAdminHandler
    {
        private readonly IConfiguration _configuration;
        private readonly IUserManagementRepository _userManagementRepository;

        public AuthAdminHandler(IConfiguration configuration, IUserManagementRepository userManagementRepository)
        {
            _configuration = configuration;
            _userManagementRepository = userManagementRepository;
        }

        public async Task<bool> ForgotPasswordAsync(ForgotPasswordRequestCapcha ps)
        {
            var user = await _userManagementRepository.GetUserByGmail(ps.Email);
            if (user == null)
            {
                return false;
            }

            string newPassword = GenerateRandomPassword(8);
            string hashedPassword = HashPassword(newPassword);

            user.PasswordHash = hashedPassword;
            await _userManagementRepository.UpdateUser(user);

            string htmlContent = EmailTemplateBuilder.BuildForgotPasswordEmail(newPassword);


            await SendEmailAsync(ps.Email, "Khôi phục mật khẩu", htmlContent);
            return true;
        }

        private async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var smtpClient = new SmtpClient(_configuration["Mail:Smtp"])
            {
                Port = int.Parse(_configuration["Mail:Port"]),
                Credentials = new System.Net.NetworkCredential(
                    _configuration["Mail:Username"],
                    _configuration["Mail:Password"]),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_configuration["Mail:From"]),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };

            mailMessage.To.Add(toEmail);
            await smtpClient.SendMailAsync(mailMessage);
        }

        private string GenerateRandomPassword(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private string GenerateJwtToken(string username, string roleName, int userId, string email)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Email, email),
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.Role, roleName),
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString())
                }),
                Expires = DateTime.UtcNow.AddHours(24),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private string HashPassword(string password)
        {
            byte[] salt = new byte[16];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000);
            byte[] hash = pbkdf2.GetBytes(20);

            byte[] hashBytes = new byte[36];
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 20);
            return Convert.ToBase64String(hashBytes);
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            byte[] hashBytes = Convert.FromBase64String(hashedPassword);
            byte[] salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);

            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000);
            byte[] computedHash = pbkdf2.GetBytes(20);

            return computedHash.SequenceEqual(hashBytes.Skip(16).Take(20));
        }
    }
}
