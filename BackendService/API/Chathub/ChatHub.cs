using Application.Interfaces;
using Application.UseCases;
using Domain.DTO.Request;
using Microsoft.AspNetCore.SignalR;


namespace API.Chathub
{
    public class ChatHub : Hub
    {
        private readonly IMessageService _messageHandler;

        public ChatHub(IMessageService messageHandler)
        {
            _messageHandler = messageHandler;
        }

        public async Task SendMessage(MessageCreateRequest user)
        {
            try
            {
                Console.WriteLine($"Hub received message: {user.MessageContent}");
                var createdMessage = await _messageHandler.createMessage(user);
                await Clients.All.SendAsync("ReceiveMessage",
                    user.ConversationId,
                    user.SenderId,
                    user.MessageContent,
                    user.SentDate,
                    createdMessage.MessageId);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Hub exception: " + ex.Message);
                throw;
            }
        }
        public async Task MarkMessagesAsRead(int currentUserId, List<int> messageIds)
        {
            try
            {
                var dtos = messageIds.Select(id => new UpdateStatusIsReadMessageDTO
                {
                    id = id
                }).ToList();

                await _messageHandler.updateStatusIsRead(dtos);
                await NotifyMessageRead(currentUserId, messageIds);

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error updating read status: " + ex.Message);
            }
        }
        public async Task NotifyMessageRead(int currentUserId, List<int> messageIds)
        {
            try
            {
                Console.WriteLine($"User {currentUserId} has read messages: {string.Join(", ", messageIds)}");
                await Clients.Others.SendAsync("UpdateMessageStatus", currentUserId, messageIds);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error notifying message read: {ex.Message}");
                throw;
            }
        }



    }

}