using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Enums
{
    public enum OrderStatus
    {
        Draft,
        PendingConfirmed,
        PendingPayment,
        Confirmed,
        Delivering,
        Delivered,
        Completed,
        Canceled
    }
    public enum ReturnOrderStatus
    {
        Pending,
        Confirmed,
    }
}
