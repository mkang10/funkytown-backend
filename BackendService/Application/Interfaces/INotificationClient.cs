using Domain.DTO.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface INotificationClient
    {
        Task<bool> SendNotificationAsync(SendNotificationRequest request);
    }
}
