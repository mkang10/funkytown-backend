using Application.Interfaces;
using Domain.Entities;
using System;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Domain.DTO.Response;
using Domain.DTO.Request;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Google.Apis.Auth;
using System.Text.RegularExpressions;
using Domain.Interfaces;

namespace Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAccountRepos _accountRepos;
        private readonly IConfiguration _configuration;

        public AuthService(IAccountRepos accountRepos, IConfiguration configuration)
        {
            _accountRepos = accountRepos;
            _configuration = configuration;
        }

        public async Task<LoginResponse> AuthenticateAsync(string email, string password)
        {
            var account = await _accountRepos.GetUserByEmail(email);

            if (account != null && VerifyPassword(password, account.PasswordHash))
            {
                string token = GenerateJwtToken(account.FullName, account.Role.RoleName, account.AccountId, account.Email);

                if (account.IsActive == false)
                {
                    return new LoginResponse
                    {
                        Token = null,
                        Account = new AccountResponse
                        {
                            AccountId = account.AccountId,
                            FullName = account.FullName,
                            RoleId = account.RoleId,
                            IsActive = account.IsActive,
                            Email = account.Email
                        }
                    };
                }

                // Lấy thông tin chi tiết dựa theo RoleId
                object? roleDetails = await _accountRepos.GetRoleDetailsAsync(account);

                return new LoginResponse
                {
                    Token = token,
                    Account = new AccountResponse
                    {
                        AccountId = account.AccountId,
                        FullName = account.FullName,
                        RoleId = account.RoleId,
                        IsActive = account.IsActive,
                        Email = account.Email,
                        RoleDetails = roleDetails // Chứa thông tin chi tiết theo vai trò
                    }
                };
            }

            return null;
        }

        private List<string> ValidateRegisterDto(RegisterReq dto)
        {
            var errors = new List<string>();

            if (dto == null)
            {
                errors.Add("Dữ liệu đăng ký không được để trống.");
                return errors;
            }

            if (string.IsNullOrWhiteSpace(dto.Username) || dto.Username.Length < 3 || dto.Username.Length > 50)
                errors.Add("Username phải có độ dài từ 3 đến 50 ký tự.");

            if (string.IsNullOrWhiteSpace(dto.Email) ||
                !Regex.IsMatch(dto.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                errors.Add("Email không hợp lệ.");

            if (string.IsNullOrEmpty(dto.Password) || dto.Password.Length < 8)
                errors.Add("Password phải có ít nhất 8 ký tự.");
            else
            {
                if (!Regex.IsMatch(dto.Password, @"[A-Z]"))
                    errors.Add("Password phải chứa ít nhất một chữ hoa.");
                if (!Regex.IsMatch(dto.Password, @"[a-z]"))
                    errors.Add("Password phải chứa ít nhất một chữ thường.");
                if (!Regex.IsMatch(dto.Password, @"\d"))
                    errors.Add("Password phải chứa ít nhất một chữ số.");
                if (!Regex.IsMatch(dto.Password, @"[!@#$%^&*(),.?""{}|<>]"))
                    errors.Add("Password phải chứa ít nhất một ký tự đặc biệt.");
            }

            return errors;
        }


        public async Task<TokenResponse> RegisterAsync(RegisterReq registerDTO)
        {
            var response = new TokenResponse();

            // 1. Validate đầu vào
            var errors = ValidateRegisterDto(registerDTO);
            if (errors.Any())
            {
                response.Errors = errors;
                return response;
            }

            // 2. Kiểm tra username đã tồn tại
            var existingUserByUsername = await _accountRepos.GetUserByUsernameAsync(registerDTO.Username);
            if (existingUserByUsername != null)
            {
                response.Errors.Add("Username đã tồn tại, vui lòng chọn tên khác.");
                return response;
            }

            // 3. Kiểm tra email đã tồn tại
            var existingUserByEmail = await _accountRepos.GetUserByEmail(registerDTO.Email);
            if (existingUserByEmail != null)
            {
                response.Errors.Add("Email đã tồn tại, vui lòng sử dụng email khác.");
                return response;
            }

            // 4. Tạo Account mới
            var account = new Account
            {
                FullName = registerDTO.Username,
                PasswordHash = HashPassword(registerDTO.Password),
                Email = registerDTO.Email,
                RoleId = 1
            };
            await _accountRepos.AddUserAsync(account);

            // 5. Tạo CustomerDetail mặc định
            var customerDetail = new CustomerDetail
            {
                AccountId = account.AccountId,
                LoyaltyPoints = 0,
                MembershipLevel = "Basic",
                DateOfBirth = null,
                Gender = null,
                CustomerType = null,
                PreferredPaymentMethod = null
            };
            await _accountRepos.AddCustomerAsync(customerDetail);

            // 6. Sinh JWT và trả về
            response.Token = GenerateJwtToken(account.FullName, "user", account.AccountId, account.Email);
            return response;
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



        public async Task<LoginResponse> AuthenticateWithGoogleAsync(string idToken)
        {
            // 1. Validate idToken với Google
            var settings = new GoogleJsonWebSignature.ValidationSettings()
            {
                Audience = new[] { _configuration["Google:ClientId"] }
            };

            GoogleJsonWebSignature.Payload payload;
            try
            {
                payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
            }
            catch (InvalidJwtException)
            {
                // Token không hợp lệ
                return new LoginResponse
                {
                    Token = null,
                    Errors = new List<string> { "Google token không hợp lệ." }
                };
            }

            // 2. Lấy email và tên
            string email = payload.Email;
            string fullName = payload.Name;

            // 3. Kiểm tra email đã tồn tại chưa
            var existing = await _accountRepos.GetUserByEmail(email);
            Account account;

            if (existing != null)
            {
                // Email đã từng được đăng ký
                if (!string.IsNullOrEmpty(existing.PasswordHash))
                {
                    // Trường hợp user đã đăng ký bằng username/password
                    return new LoginResponse
                    {
                        Token = null,
                        Errors = new List<string> { "Email này đã tồn tại. Vui lòng đăng nhập bằng tài khoản và mật khẩu." }
                    };
                }

                // Trường hợp đã từng đăng nhập Google trước đó => cho phép tiếp tục
                account = existing;
            }
            else
            {
                // 4. Chưa có tài khoản, tạo mới
                account = new Account
                {
                    FullName = fullName,
                    Email = email,
                    PasswordHash = null,    // sẽ login bằng Google
                    RoleId = 1,
                    IsActive = true
                };
                await _accountRepos.AddUserAsync(account);

                // Tạo detail mặc định
                var detail = new CustomerDetail
                {
                    AccountId = account.AccountId,
                    LoyaltyPoints = 0,
                    MembershipLevel = "Basic",
                    DateOfBirth = null,
                    Gender = null,
                    CustomerType = null,
                    PreferredPaymentMethod = null
                };
                await _accountRepos.AddCustomerAsync(detail);
            }

            // 5. Kiểm tra active
            if (!(bool)account.IsActive)
            {
                return new LoginResponse
                {
                    Token = null,
                    Account = new AccountResponse
                    {
                        AccountId = account.AccountId,
                        FullName = account.FullName,
                        RoleId = account.RoleId,
                        IsActive = account.IsActive,
                        Email = account.Email
                    },
                    Errors = new List<string> { "Tài khoản của bạn đang bị khóa." }
                };
            }

            // 6. Sinh JWT
            string token = GenerateJwtToken(
                account.FullName,
                "user",
                account.AccountId,
                account.Email
            );

            var roleDetails = await _accountRepos.GetRoleDetailsAsync(account);

            return new LoginResponse
            {
                Token = token,
                Account = new AccountResponse
                {
                    AccountId = account.AccountId,
                    FullName = account.FullName,
                    RoleId = account.RoleId,
                    IsActive = account.IsActive,
                    Email = account.Email,
                    RoleDetails = roleDetails
                }
            };
        }


    }
}
