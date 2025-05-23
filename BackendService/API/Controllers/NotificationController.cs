
using Application.UseCases;
using Domain.DTO.Request;
using Domain.DTO.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    public class NotificationController : ControllerBase
    {
        private readonly SendNotificationHandler _sendNotificationHandler;
        private readonly GetNotificationsHandler _getNotificationsHandler;
        private readonly MarkNotificationAsReadHandler _markNotificationAsReadHandler;

        public NotificationController(
            SendNotificationHandler sendNotificationHandler,
            GetNotificationsHandler getNotificationsHandler,
            MarkNotificationAsReadHandler markNotificationAsReadHandler)
        {
            _sendNotificationHandler = sendNotificationHandler;
            _getNotificationsHandler = getNotificationsHandler;
            _markNotificationAsReadHandler = markNotificationAsReadHandler;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendNotification([FromBody] SendNotificationRequest request)
        {
            var notification = await _sendNotificationHandler.Handle(request);
            var responseDTO = new NotificationResponse
            {
                NotificationID = notification.NotificationId,
                Title = notification.Title,
                Content = notification.Content,
                NotificationType = notification.NotificationType,
                IsRead = notification.IsRead,
                CreatedDate = notification.CreatedDate
            };
            return Ok(new ResponseDTO<NotificationResponse>(responseDTO, true, "Notification sent successfully"));
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserNotifications(int userId)
        {
            var notifications = await _getNotificationsHandler.Handle(userId);
            var responseList = notifications.Select(n => new NotificationResponse
            {
                NotificationID = n.NotificationId,
                Title = n.Title,
                Content = n.Content,
                NotificationType = n.NotificationType,
                IsRead = n.IsRead,
                CreatedDate = n.CreatedDate
            }).ToList();

            return Ok(new ResponseDTO<List<NotificationResponse>>(responseList, true, "Success"));
        }

        [HttpPost("mark-as-read/{notificationId}")]
        public async Task<IActionResult> MarkAsRead(int notificationId)
        {
            await _markNotificationAsReadHandler.Handle(notificationId);
            return Ok(new ResponseDTO<bool>(true, true, "Notification marked as read"));
        }
    }
}
