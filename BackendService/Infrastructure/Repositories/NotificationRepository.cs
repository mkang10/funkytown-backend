using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly FtownContext _context;

        public NotificationRepository(FtownContext context)
        {
            _context = context;
        }

        public async Task<List<Notification>> GetNotificationsByUserIdAsync(int userId)
        {
            return await _context.Notifications
                .Where(n => n.AccountId == userId)
                .OrderByDescending(n => n.CreatedDate)
                .ToListAsync();
        }

        public async Task AddNotificationAsync(Notification notification)
        {
            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();
        }

        public async Task MarkAsReadAsync(int notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification != null)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }
    }
}
