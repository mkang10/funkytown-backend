using Application.Interfaces;
using AutoMapper;
using Azure.Core;
using Domain.Commons;
using Domain.DTO.Request;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class ConversationHandler : IConversationService
    {
        private readonly IMessageRepository _message;
        private readonly IMapper _mapper;

        public ConversationHandler(IMessageRepository message, IMapper mapper)
        {
            _message = message;
            _mapper = mapper;
        }

        public async Task<ConversationCreateRequest> createConversation(ConversationCreateRequest user)
        {
            try
            {
                // checking data null or not 
                if (user.ConversationName == null)
                {
                    throw new Exception($"Name required");
                }
                if (!user.ParticipantIds.Any())
                {
                    throw new Exception($"No user joined the conversation!");
                }
                var participantSet = new HashSet<int>();
                var shopManagerDetailIds = await _message.GetAllShopManagerId();
                var staffDetail = await _message.GetAllStaffId();
                //checking account exsist
                foreach (var participantId in user.ParticipantIds)
                {
                    if (!shopManagerDetailIds.Contains(participantId) && !staffDetail.Contains(participantId))
                    {
                        throw new Exception($"Participant ID {participantId} does not exist in ShopManagerDetails or StaffDetails table");
                    }
                    if (!participantSet.Add(participantId))
                    {
                        throw new Exception("Same user");
                    }
                }
                //Create conversation
                var map = _mapper.Map<Conversation>(user);
                var userCreate = await _message.CreateConversation(map);
                var result = _mapper.Map<ConversationCreateRequest>(userCreate);
                //adding user to group
                foreach (var participantId in user.ParticipantIds)
                {
                    var participant = new ConversationParticipant
                    {
                        ConversationId = userCreate.ConversationId, 
                        AccountId = participantId,
                        JoinedDate = DateTime.UtcNow
                    };

                    await _message.CreateConversationParticipants(participant);
                }
                return result; 
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<bool> deleteConversation(int id)
        {
            try
            {
                var user = await _message.GetConversationById(id);
                var user2 = await _message.GetConversationParticipantById(id);

                if (user == null)
                {
                    throw new Exception($"Conversation {id} does not exist");
                }
                await _message.DeleteConversationParticipant(user2);
                await _message.DeleteConversation(user);
                

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        private async Task<bool> deleteConversationParticipants(int id)
        {
            try
            {
                var user = await _message.GetConversationById(id);
                if (user == null)
                {
                    throw new Exception($"Conversation {id} does not exist");
                }

                await _message.DeleteConversation(user);

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<Pagination<ConversationRequest>> GetAllConversationServiceByAccountId(int id, PaginationParameter paginationParameter)
        {
            try
            {                
                var trips = await _message.GetAllConversationByAccountId(id,paginationParameter);
                if (!trips.Any())
                {
                    throw new Exception("No data!");
                }

                var tripModels = _mapper.Map<List<ConversationRequest>>(trips);
                var paginationResult = new Pagination<ConversationRequest>(tripModels,
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
    }
}
