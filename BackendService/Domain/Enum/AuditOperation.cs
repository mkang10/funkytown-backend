using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Enums
{
    public enum AuditOperation
    {
        CreateOrder,
        UpdateOrderInfo,
        ChangeStatus,
        ConfirmOrder,
        CancelOrder,
        CompleteOrder,
        AssignToManager,
        CreateReturnOrder,
        UpdateStatus
    }
}
