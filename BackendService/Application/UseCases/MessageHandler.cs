using Application.Interfaces;
using AutoMapper;
using Domain.Commons;
using Domain.DTO.Request;
using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class MessageHandler : IMessageService
    {
        private readonly IMessageRepository _message;
        private readonly IMapper _mapper;

        public MessageHandler(IMessageRepository message, IMapper mapper)
        {
            _message = message;
            _mapper = mapper;
        }
        public async Task<MessageCreateRequest> createMessage(MessageCreateRequest user)
        {
            try
            {
                var map = _mapper.Map<Message>(user);
                var userCreate = await _message.CreateMessage(map);
                var result = _mapper.Map<MessageCreateRequest>(userCreate);                
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<bool> deleteMessage(int id)
        {
            try
            {
                var user = await _message.GetMessageById(id);

                if (user == null)
                {
                    throw new Exception($"Message {id} does not exist");
                }
                await _message.DeleteMessage(user);


                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<Pagination<MessageRequest>> GetAllMessageServiceByConversationId(int id, PaginationParameter paginationParameter)
        {
            try
            {
                var trips = await _message.GetAllMessageByConservationId(id, paginationParameter);
                if (!trips.Any())
                {
                    throw new Exception("No data!");
                }

                var tripModels = _mapper.Map<List<MessageRequest>>(trips);
                var paginationResult = new Pagination<MessageRequest>(tripModels,
                    trips.TotalCount,
                    trips.CurrentPage,
                    trips.PageSize);

                return paginationResult;
            }

            catch (Exception ex)
            {
                throw new Exception("An error occurred: " + ex.Message);
            }
        }
        public async Task<List<MessageCreateRequest>> updateStatusIsRead(List<UpdateStatusIsReadMessageDTO> dtos)
        {
            var ids = dtos.Select(x => x.id).ToList();

            var messages = await _message.GetMessagesByIdsAsync(ids);

            if (messages == null || messages.Count == 0)
                return new List<MessageCreateRequest>();

            foreach (var message in messages)
            {
                message.IsRead = true;
            }

            await _message.UpdateStatusIsReadRepository(messages);

            return _mapper.Map<List<MessageCreateRequest>>(messages);
        }
    }
}
