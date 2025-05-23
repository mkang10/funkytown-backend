using Application.Interfaces;
using AutoMapper;
using Azure.Core;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Domain.Commons;
using Domain.DTO.Request;
using Domain.Entities;
using Domain.Enum;
using Domain.Interfaces;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Order = Domain.Entities.Order;

namespace Application.UseCases
{
    public class FeedbackHandler : ICommentService
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IOrderDetailRepository _orderDetailRepository;
        private readonly Cloudinary _cloudinary;


        private const string CacheKey = "Data";
        private readonly IMapper _mapper;
        private readonly IConnectionMultiplexer _redis;
        private readonly HttpClient _httpClient;

        public FeedbackHandler(ICommentRepository commentRepository,
            IOrderDetailRepository orderDetailRepository, Cloudinary cloudinary,
            IMapper mapper, IConnectionMultiplexer redis, HttpClient httpClient)
        {
            _commentRepository = commentRepository;
            _orderDetailRepository = orderDetailRepository;
            _cloudinary = cloudinary;
            _mapper = mapper;
            _redis = redis;
            _httpClient = httpClient;
        }

        public async Task<List<FeedbackRequestDTO>> CreateMultiple(List<CreateFeedBackArrayRequestDTO> feedbackRequests)
        {
            var createdFeedbacks = new List<FeedbackRequestDTO>();
            int? orderIdToUpdate = null;

            foreach (var request in feedbackRequests)
            {
                // Kiểm tra OrderDetailId hợp lệ
                if (!request.orderDetailId.HasValue)
                    continue; // Bỏ qua feedback không hợp lệ

                // Lấy thông tin OrderDetail từ repository
                var orderDetail = await _orderDetailRepository.GetOrderDetailById(request.orderDetailId.Value);
                var orderData = await _orderDetailRepository.GetOrderStatuslById(orderDetail.OrderId);
                string ImageString = null;

                // Kiểm tra đơn hàng có trạng thái "completed"
                if (orderData?.Status?.Equals("completed", StringComparison.OrdinalIgnoreCase) != true)
                    continue; // Bỏ qua nếu đơn hàng chưa hoàn thành
                if (orderData?.IsFeedback == true)
                    continue;
                orderIdToUpdate = orderDetail.OrderId;

                if (request.ImageFile != null && request.ImageFile.Length > 0)
                {
                    var uploadParams = new ImageUploadParams
                    {
                        File = new FileDescription(request.ImageFile.FileName, request.ImageFile.OpenReadStream()),
                        UseFilename = true,
                        UniqueFilename = true,
                        Overwrite = true
                    };

                    var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                    if (uploadResult.StatusCode != HttpStatusCode.OK)
                    {
                        throw new Exception("Error Picture!");
                    }
                    ImageString = uploadResult.SecureUrl.ToString();
                }

                // Map DTO sang entity và lưu vào database
                var feedbackEntity = _mapper.Map<Feedback>(request);
                feedbackEntity.ImagePath = ImageString?.ToString();
                var createdFeedback = await _commentRepository.CreateFeedback(feedbackEntity);
                // Thêm feedback đã tạo vào danh sách kết quả
                var feedbackGet = await _commentRepository.GetFeedBackById(createdFeedback.FeedbackId);
                createdFeedback.Account.FullName = feedbackGet.Account.FullName;
                createdFeedback.Product.Name = feedbackGet.Product.Name;
                createdFeedbacks.Add(_mapper.Map<FeedbackRequestDTO>(createdFeedback));
            }
            if (orderIdToUpdate.HasValue)
            {
                var orderToUpdate = await _orderDetailRepository.GetOrderStatuslById(orderIdToUpdate.Value);
                orderToUpdate.IsFeedback = true;
                await _commentRepository.UpdateStatusIsFeedback(orderToUpdate);
            }
            return createdFeedbacks; // Trả về danh sách feedback đã tạo
        }



        public async Task<bool> Delete(int id)
        {
            try
            {
                var user = await _commentRepository.GetFeedBackById(id);
                if (user == null)
                {
                    throw new Exception($"Feedback {id} does not exist");
                }

                await _commentRepository.DeleteFeedback(user);
                // call redis and delete 1 of item in cache
                var db = _redis.GetDatabase();
                await db.KeyDeleteAsync("Data");

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<Pagination<FeedbackRequestDTO>> GetAllFeedbackByProductId(int id, PaginationParameter paginationParameter)
        {
            var cacheKey = $"Feedback_Product_{id}_{paginationParameter.PageIndex}_{paginationParameter.PageSize}";
            var db = _redis.GetDatabase();

            // (tuỳ chọn) đọc cache nếu có
            var cached = await db.StringGetAsync(cacheKey);
            if (cached.HasValue)
            {
                return JsonConvert.DeserializeObject<Pagination<FeedbackRequestDTO>>(cached);
            }

            // Lấy từ DB
            var trips = await _commentRepository.GettAllFeedbackByProductId(id, paginationParameter);

            // Map sang DTO (trips có thể rỗng)
            var tripModels = _mapper.Map<List<FeedbackRequestDTO>>(trips);

            var paginationResult = new Pagination<FeedbackRequestDTO>(
                tripModels,
                trips.TotalCount,
                trips.CurrentPage,
                trips.PageSize
            );

            // Ghi cache
            await db.StringSetAsync(cacheKey,
                JsonConvert.SerializeObject(paginationResult),
                TimeSpan.FromMinutes(300)
            );

            return paginationResult;
        }

        public async Task<FeedbackRequestDTO> GetById(int id)
        {
            try
            {
                var data = await _commentRepository.GetFeedBackById(id);
                if (data == null)
                {
                    throw new Exception("Feedback does not exsist!");
                }
                var dataModel = _mapper.Map<FeedbackRequestDTO>(data);

                return dataModel;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occur: " + ex.Message);
            }
        }

        public async Task<Pagination<FeedbackRequestDTO>> GettAllFeedbackByAccountId(int id, PaginationParameter paginationParameter)
        {
            try
            {
                var cacheKey = "Data";
                var db = _redis.GetDatabase();

                // Nếu có dữ liệu trong cache, có thể deserialize và trả về (đoạn này đang comment)
                // var cachedData = await db.StringGetAsync(cacheKey);
                // if (cachedData.HasValue)
                // {
                //     var cachedResult = JsonConvert.DeserializeObject<Pagination<FeedbackRequestDTO>>(cachedData);
                //     return cachedResult;
                // }

                // Lấy dữ liệu từ repository
                var trips = await _commentRepository.GettAllCommentByAccountId(id, paginationParameter);

                // Kiểm tra null cho trips và trips.Items
                if (!trips.Any())
                {
                    throw new Exception("No data!");
                }

                // Map danh sách Feedback sang FeedbackRequestDTO
                var tripModels = _mapper.Map<List<FeedbackRequestDTO>>(trips);

                // Tạo đối tượng Pagination<FeedbackRequestDTO> mới dựa trên các thuộc tính của trips
                var paginationResult = new Pagination<FeedbackRequestDTO>(
                    tripModels,
                    trips.TotalCount,
                    trips.CurrentPage,
                    trips.PageSize);

                // Ghi cache kết quả vào Redis trong 300 phút
                await db.StringSetAsync(cacheKey, JsonConvert.SerializeObject(paginationResult), TimeSpan.FromMinutes(300));

                return paginationResult;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred: " + ex.Message);
            }
        }


        public async Task<bool> Update(int id, UpdateFeedbackRequestDTO user)
        {
            try
            {
                var userData = await _commentRepository.GetFeedBackById(id);
                if (userData == null)
                {
                    throw new Exception("No data!");
                }

                _mapper.Map(user, userData);

                await _commentRepository.UpdateFeedback(userData);

                var db = _redis.GetDatabase();
                //delete old cache
                await db.KeyDeleteAsync("Data");
                var paginationParameter = new PaginationParameter();
                // call and wite new cache to redis
                var updatedUsers = await _commentRepository.GettAllFeedbackByProductId(user.ProductId, paginationParameter);
                await db.StringSetAsync("Data", JsonConvert.SerializeObject(updatedUsers), TimeSpan.FromMinutes(300));

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred: " + ex.Message);
            }
        }

        public async Task<FeedbackRequestDTO> Create(CreateFeedBackRequestDTO feedbackRequests)
        {
            string ImageString = null;

            var orderDetail = await _orderDetailRepository.GetOrderStatuslById(feedbackRequests.orderDetailId.Value);
            if (orderDetail.AccountId != feedbackRequests.AccountId)
            {
                throw new Exception("Sai chủ đơn hàng!");
            }
            if (orderDetail.Status != StatusSuccess.completed.ToString())
            {
                throw new Exception("Đơn hàng chưa hoàn tất!");
            }
            if (orderDetail.IsFeedback == true)
            {
                throw new Exception("Đơn hàng đã được feedback, không thể tạo!");
            }

            if (feedbackRequests.ImgFile != null && feedbackRequests.ImgFile.Length > 0)
            {
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(feedbackRequests.ImgFile.FileName, feedbackRequests.ImgFile.OpenReadStream()),
                    UseFilename = true,
                    UniqueFilename = true,
                    Overwrite = true
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.StatusCode != HttpStatusCode.OK)
                {
                    throw new Exception("Error Picture!");
                }
                ImageString = uploadResult.SecureUrl.ToString();
            }

            // Map DTO sang entity và lưu vào database
            var feedbackEntity = _mapper.Map<Feedback>(feedbackRequests);
            feedbackEntity.ImagePath = ImageString;
            var createdFeedback = await _commentRepository.CreateFeedback(feedbackEntity);
            var result = _mapper.Map<FeedbackRequestDTO>(createdFeedback);
            return result;
        }
    }
}
