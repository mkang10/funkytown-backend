using Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.HelperServices
{
    public class AssignmentSettingService : IAssignmentSettingService
    {
        private int _defaultShopManagerId = 2; // Default ban đầu
        private int _defaultStaffId = 5; // Default ban đầu

        public int DefaultShopManagerId => _defaultShopManagerId;
        public int DefaultStaffId => _defaultStaffId;

        public void UpdateDefaultAssignment(int shopManagerId, int staffId)
        {
            _defaultShopManagerId = shopManagerId;
            _defaultStaffId = staffId;
        }
    }

}
