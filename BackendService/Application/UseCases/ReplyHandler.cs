using Application.Interfaces;
using AutoMapper;
using Domain.Commons;
using Domain.DTO.Request;
using Domain.Entities;
using Domain.Interfaces;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class ReplyHandler : IReplyFeedbackService
    {
        private readonly ICommentRepository _commentRepository;
        private const string CacheKey = "Data";
        private readonly IMapper _mapper;
        private readonly IConnectionMultiplexer _redis;

        public ReplyHandler(ICommentRepository commentRepository, IMapper mapper, IConnectionMultiplexer redis)
        {
            _commentRepository = commentRepository;
            _mapper = mapper;
            _redis = redis;
        }

        public async Task<CreateReplyRequestDTO> Create(CreateReplyRequestDTO user)
        {
            try
            {
                var map = _mapper.Map<ReplyFeedback>(user);
                var userCreate = await _commentRepository.CreateReply(map);
                var result = _mapper.Map<CreateReplyRequestDTO>(userCreate);
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<bool> Delete(int id)
        {
            try
            {
                var user = await _commentRepository.GetReplyFeedBackById(id);
                if (user == null)
                {
                    throw new Exception($"Reply {id} does not exist");
                }

                await _commentRepository.DeleteReply(user);
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

        public async Task<ReplyRequestDTO> GetById(int id)
        {
            try
            {
                var data = await _commentRepository.GetReplyFeedBackById(id);
                if (data == null)
                {
                    throw new Exception("Reply does not exsist!");
                }
                var dataModel = _mapper.Map<ReplyRequestDTO>(data);

                return dataModel;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occur: " + ex.Message);
            }
        }

        public async Task<Pagination<ReplyRequestDTO>> GettAllReplyByFeedbackId(int id, PaginationParameter paginationParameter)
        {
            try
            {
                var cacheKey = "Data";
                var db = _redis.GetDatabase();

                // check cache null or not ?
                //var cachedData = await db.StringGetAsync(cacheKey);
                //if (cachedData.HasValue)
                //{
                //    // if not null , deserialize object
                //    var cachedResult = JsonConvert.DeserializeObject<Pagination<ReplyRequestDTO>>(cachedData);
                //    return cachedResult;
                //}

                // if null cache, get data from db and write it down cache
                var trips = await _commentRepository.GettAllReplyByFeedbackId(id, paginationParameter);
                if (!trips.Any())
                {
                    throw new Exception("No data!");
                }

                var tripModels = _mapper.Map<List<ReplyRequestDTO>>(trips);

                // write down cache
                var paginationResult = new Pagination<ReplyRequestDTO>(tripModels,
                    trips.TotalCount,
                    trips.CurrentPage,
                    trips.PageSize);
                await db.StringSetAsync(cacheKey, JsonConvert.SerializeObject(paginationResult), TimeSpan.FromMinutes(300));

                return paginationResult;
            }

            catch (Exception ex)
            {
                throw new Exception("An error occurred: " + ex.Message);
            }
        }

        public async Task<bool> Update(int id, UpdateReplyRequestDTO user)
        {
            try
            {
                var userData = await _commentRepository.GetReplyFeedBackById(id);
                if (userData == null)
                {
                    throw new Exception("No data!");
                }

                _mapper.Map(user, userData);

                await _commentRepository.UpdateReply(userData);

                var db = _redis.GetDatabase();
                //delete old cache
                await db.KeyDeleteAsync("Data");
                var paginationParameter = new PaginationParameter();
                // call and wite new cache to redis
                var updatedUsers = await _commentRepository.GettAllReplyByFeedbackId(userData.FeedbackId, paginationParameter);
                await db.StringSetAsync("Data", JsonConvert.SerializeObject(updatedUsers), TimeSpan.FromMinutes(300));

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred: " + ex.Message);
            }
        }
    }
}
