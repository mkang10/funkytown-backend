using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IAssignmentSettingService
    {
        int DefaultShopManagerId { get; }
        int DefaultStaffId { get; }
        void UpdateDefaultAssignment(int shopManagerId, int staffId);
    }

}
