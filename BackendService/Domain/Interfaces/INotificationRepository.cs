using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface INotificationRepository
    {
        Task<List<Notification>> GetNotificationsByUserIdAsync(int userId);
        Task AddNotificationAsync(Notification notification);
        Task MarkAsReadAsync(int notificationId);
    }
}
