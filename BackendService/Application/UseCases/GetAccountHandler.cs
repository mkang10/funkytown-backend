//using Application.DTO.Request;
//using Application.Interfaces;
//using AutoMapper;
//using CloudinaryDotNet;
//using CloudinaryDotNet.Actions;
//using Domain.Commons;
//using Domain.Entities;
//using Domain.Interfaces;
//using Microsoft.EntityFrameworkCore.Metadata;
//using Newtonsoft.Json;
//using StackExchange.Redis;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net;
//using System.Text;
//using System.Threading.Tasks;

//namespace Application.UseCases
//{
//    public class GetAccountHandler : IUserManagementService
//    {
//        private readonly IConnectionMultiplexer _redis;
//        private readonly IUserManagementRepository _userManagementRepository;
//        private const string CacheKey = "UserAccounts";
//        private readonly IMapper _mapper;
//        private readonly Cloudinary _cloudinary;

//        public GetAccountHandler(IConnectionMultiplexer redis, IUserManagementRepository userManagementRepository, IMapper mapper, Cloudinary cloudinary)
//        {
//            _redis = redis;
//            _userManagementRepository = userManagementRepository;
//            _mapper = mapper;
//            _cloudinary = cloudinary;
//        }

//        public async Task<bool> banUser(int id, BanUserRequestDTO user)
//        {
//            try
//            {
//                var userData = await _userManagementRepository.GetUserById(id);
//                if (userData == null)
//                {
//                    return false;
//                }

//                _mapper.Map(user, userData);
//                await _userManagementRepository.UpdateUser(userData);
//                return true;
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"Fail to ban user {ex.Message}");
//                return false;
//            }
//        }

//        public async Task<UserRequestDTO> createUser(CreateUserRequestWithPasswordDTO user)
//        {
//            try
//            {
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
//                user.PasswordHash = EncryptPassword("funkytown123");

//                var map = _mapper.Map<Domain.Entities.Account>(user);
//                var userCreate = await _userManagementRepository.CreateUser(map);
//                var result = _mapper.Map<UserRequestDTO>(userCreate);
//                return result;
//            }
//            catch (Exception ex)
//            {
//                throw new Exception(ex.Message);
//            }
//        }
//        private string EncryptPassword(string password)
//        {
//            using (var sha256 = System.Security.Cryptography.SHA256.Create())
//            {
//                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
//                return Convert.ToBase64String(bytes);
//            }
//        }

//        public async Task<bool> deleteUser(int id)
//        {
//            try
//            {
//                var user = await _userManagementRepository.GetUserById(id);
//                if (user == null)
//                {
//                    throw new Exception($"User {id} does not exist");
//                }

//                await _userManagementRepository.DeleteUser(user);
//                // call redis and delete 1 of item in cache
//                var db = _redis.GetDatabase();
//                await db.KeyDeleteAsync("UserAccounts");

//                return true;
//            }
//            catch (Exception ex)
//            {
//                throw new Exception(ex.Message);
//            }
//        }

//        public async Task<UserRequestDTO> getAccountInfoById(int id)
//        {
//            try
//            {
//                var data = await _userManagementRepository.GetUserById(id);
//                if (data == null)
//                {
//                    throw new Exception("User does not exsist!");
//                }
//                var dataModel = _mapper.Map<UserRequestDTO>(data);

//                return dataModel;
//            }
//            catch (Exception ex)
//            {
//                throw new Exception("An error occur: " + ex.Message);
//            }
//        }

//        public async Task<Pagination<UserRequestDTO>> GetAllUserAscyn(PaginationParameter paginationParameter)
//        {
//            try
//            {
//                var cacheKey = "UserAccounts";
//                var db = _redis.GetDatabase();

//                // check cache null or not ?
//                var cachedData = await db.StringGetAsync(cacheKey);
//                if (cachedData.HasValue)
//                {
//                    // if not null , deserialize object
//                    var cachedResult = JsonConvert.DeserializeObject<Pagination<UserRequestDTO>>(cachedData);
//                    return cachedResult;
//                }

//                // if null cache, get data from db and write it down cache
//                var trips = await _userManagementRepository.GetAllUser(paginationParameter);
//                if (!trips.Any())
//                {
//                    throw new Exception("No data!");
//                }

//                var tripModels = _mapper.Map<List<UserRequestDTO>>(trips);

//                // write down cache
//                var paginationResult = new Pagination<UserRequestDTO>(tripModels,
//                    trips.TotalCount,
//                    trips.CurrentPage,
//                    trips.PageSize);
//                await db.StringSetAsync(cacheKey, JsonConvert.SerializeObject(paginationResult), TimeSpan.FromMinutes(300));

//                return paginationResult;
//            }

//            catch (Exception ex)
//            {
//                throw new Exception("An error occurred: " + ex.Message);
//            }
//        }


//        public async Task<bool> updateUser(int id, CreateUserRequestWithPasswordDTO user)
//        {
//            try
//            {
//                var userData = await _userManagementRepository.GetUserById(id);
//                if (userData == null)
//                {
//                    throw new Exception("No data!");
//                }

//                _mapper.Map(user, userData);

//                await _userManagementRepository.UpdateUser(userData);

//                var db = _redis.GetDatabase();
//                //delete old cache
//                await db.KeyDeleteAsync("UserAccounts");
//                var paginationParameter = new PaginationParameter();
//                // call and wite new cache to redis
//                var updatedUsers = await _userManagementRepository.GetAllUser(paginationParameter);
//                await db.StringSetAsync("UserAccounts", JsonConvert.SerializeObject(updatedUsers), TimeSpan.FromMinutes(300));

//                return true;
//            }
//            catch (Exception ex)
//            {
//                throw new Exception("An error occurred: " + ex.Message);
//            }
//        }

//        public async Task<CreateShopmanagerDetailRequest> CreateShopmanagerDetail(CreateShopmanagerDetailRequest user)
//        {
//            try
//            {
//                var userData = await _userManagementRepository.GetShopManagerdetailById(user.AccountId);
//                var roleid = await _userManagementRepository.GetUserById(user.AccountId);
//                if (userData != null)
//                {
//                    throw new Exception("Already exsist");
//                }
//                // check roleid
//                if (roleid.RoleId != 2 && roleid.RoleId != 3)
//                {
//                    throw new Exception("Wrong roleId");
//                }
//                // Encrypte

//                var map = _mapper.Map<ShopManagerDetail>(user);
//                var userCreate = await _userManagementRepository.CreateShopmanagerDetail(map);
//                var result = _mapper.Map<CreateShopmanagerDetailRequest>(userCreate);
//                return result;
//            }
//            catch (Exception ex)
//            {
//                throw new Exception(ex.Message);
//            }
//        }

//        public async Task<bool> UpdateShopmanagerDetail(int id, CreateShopmanagerDetailRequest user)
//        {
//            try
//            {
//                var userData = await _userManagementRepository.GetShopManagerdetailById(id);
//                if (userData == null)
//                {
//                    throw new Exception("No data!");
//                }

//                _mapper.Map(user, userData);

//                await _userManagementRepository.UpdateShopmanagerDetail(userData);

//                return true;
//            }
//            catch (Exception ex)
//            {
//                throw new Exception("An error occurred: " + ex.Message);
//            }
//        }

//        public Task<CreateShopmanagerDetailRequest> getShopmanagerDetaibyid(int id)
//        {
//            throw new NotImplementedException();
//        }
//    }
//}
