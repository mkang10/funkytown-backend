using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class MarkNotificationAsReadHandler
    {
        private readonly INotificationRepository _notificationRepository;

        public MarkNotificationAsReadHandler(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        public async Task Handle(int notificationId)
        {
            await _notificationRepository.MarkAsReadAsync(notificationId);
        }
    }

}
