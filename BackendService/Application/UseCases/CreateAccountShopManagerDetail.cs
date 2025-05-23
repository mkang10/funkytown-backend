//using Application.DTO.Request;
//using CloudinaryDotNet.Actions;
//using CloudinaryDotNet;
//using Domain.Interfaces;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net;
//using System.Text;
//using System.Threading.Tasks;
//using AutoMapper;
//using Microsoft.IdentityModel.Tokens;
//using System.IdentityModel.Tokens.Jwt;
//using System.Security.Claims;
//using System.Security.Cryptography;
//using Microsoft.Extensions.Configuration;
//using Domain.Entities;

//namespace Application.UseCases
//{
//    public class CreateAccountShopManagerDetail
//    {
//        private readonly IUserManagementRepository _userManagementRepository;
//        private readonly IMapper _mapper;
//        private readonly Cloudinary _cloudinary;
//        private readonly IConfiguration _configuration;

//        public CreateAccountShopManagerDetail(IUserManagementRepository userManagementRepository, IMapper mapper, Cloudinary cloudinary, IConfiguration configuration)
//        {
//            _userManagementRepository = userManagementRepository;
//            _mapper = mapper;
//            _cloudinary = cloudinary;
//            _configuration = configuration;
//        }

//        public async Task<CreateUserFullResponseDTO> createUserStaffOrShopManager(CreateUserRequestWithPasswordDTO user)
//        {
//            try
//            {
//                var dataCheck = _userManagementRepository.GetUserByGmail(user.Email);
//                if (dataCheck != null)
//                {
//                    throw new Exception("User email ton tai!");
//                }
//                // check roleid
//                if (user.RoleId != 2 && user.RoleId != 3)
//                {
//                    throw new Exception("Wrong roleId");
//                }

//                if (user.ImgFile != null && user.ImgFile.Length > 0)
//                {
//                    var uploadParams = new ImageUploadParams
//                    {
//                        File = new FileDescription(user.ImgFile.FileName, user.ImgFile.OpenReadStream()),
//                        UseFilename = true,
//                        UniqueFilename = true,
//                        Overwrite = true
//                    };

//                    var uploadResult = await _cloudinary.UploadAsync(uploadParams);

//                    if (uploadResult.StatusCode != HttpStatusCode.OK)
//                    {
//                        throw new Exception("Error Picture!");
//                    }
//                    user.ImagePath = uploadResult.SecureUrl.ToString();

//                }

//                // Encrypte
//                user.PasswordHash = HashPassword("funkytown123");

//                var map = _mapper.Map<Domain.Entities.Account>(user);
//                var userCreate = await _userManagementRepository.CreateUser(map);
//                var result = _mapper.Map<UserRequestDTO>(userCreate);
//                string token = null;
//                if (result.RoleId == 2)
//                {
//                     token = GenerateJwtToken(result.FullName, "shopmanager", result.AccountId, result.Email);
//                }else if(result.RoleId == 3)
//                {
//                    token = GenerateJwtToken(result.FullName, "staff", result.AccountId, result.Email);

//                }
//                var response = new CreateUserFullResponseDTO
//                {
//                    User = result,
//                    Token = token,
//                };

//                if (result.RoleId == 2) // Shopmanager
//                {
//                    var shopManagerDetail = new ShopManagerDetail
//                    {
//                        AccountId = result.AccountId,
//                        ManagedDate = DateTime.Now
//                    };
//                    var shopmanager = await _userManagementRepository.CreateShopmanagerDetail(shopManagerDetail);
//                }
//                else if (result.RoleId == 3) // Staff
//                {
//                    var staffDetail = new StaffDetail
//                    {
//                        AccountId = result.AccountId,
//                        JoinDate = DateTime.Now,
//                    };
//                    var staff = await _userManagementRepository.CreateStaffDetail(staffDetail);
//                }

//                return response;
//            }
//            catch (Exception ex)
//            {
//                throw new Exception(ex.Message);
//            }
//        }

//        private string GenerateJwtToken(string username, string roleName, int userId, string email)
//        {
//            var tokenHandler = new JwtSecurityTokenHandler();
//            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);

//            var tokenDescriptor = new SecurityTokenDescriptor
//            {
//                Subject = new ClaimsIdentity(new[]
//                {
//                    new Claim(ClaimTypes.Email, email),
//                    new Claim(ClaimTypes.Name, username),
//                    new Claim(ClaimTypes.Role, roleName),
//                    new Claim(ClaimTypes.NameIdentifier, userId.ToString())
//                }),
//                Expires = DateTime.Now.AddHours(24),
//                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
//            };

//            var token = tokenHandler.CreateToken(tokenDescriptor);
//            return tokenHandler.WriteToken(token);
//        }

//        private string HashPassword(string password)
//        {
//            byte[] salt = new byte[16];
//            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
//            {
//                rng.GetBytes(salt);
//            }

//            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000);
//            byte[] hash = pbkdf2.GetBytes(20);

//            byte[] hashBytes = new byte[36];
//            Array.Copy(salt, 0, hashBytes, 0, 16);
//            Array.Copy(hash, 0, hashBytes, 16, 20); 
//            return Convert.ToBase64String(hashBytes);
//        }

//        private bool VerifyPassword(string password, string hashedPassword)
//        {
//            byte[] hashBytes = Convert.FromBase64String(hashedPassword);
//            byte[] salt = new byte[16];
//            Array.Copy(hashBytes, 0, salt, 0, 16);

//            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000);
//            byte[] computedHash = pbkdf2.GetBytes(20);

//            return computedHash.SequenceEqual(hashBytes.Skip(16).Take(20));
//        }
//    }
//}
