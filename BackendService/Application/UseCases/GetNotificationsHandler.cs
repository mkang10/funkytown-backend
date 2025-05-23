using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class GetNotificationsHandler
    {
        private readonly INotificationRepository _notificationRepository;

        public GetNotificationsHandler(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        public async Task<List<Notification>> Handle(int userId)
        {
            return await _notificationRepository.GetNotificationsByUserIdAsync(userId);
        }
    }

}
