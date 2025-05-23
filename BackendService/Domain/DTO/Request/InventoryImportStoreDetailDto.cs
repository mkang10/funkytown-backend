using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Request
{
    public class InventoryImportStoreDetailDto
    {
        public int ImportId { get; set; }

        public int ImportStoreId { get; set; }
        public int ImportDetailId { get; set; }
        public int WareHouseId { get; set; }
        public int AllocatedQuantity { get; set; }
        public string? Status { get; set; }
        public string? Comments { get; set; }
        public int? StaffDetailId { get; set; }
        public string ProductName { get; set; }

        public string WareHouseName { get; set; } = string.Empty;
        public string? StaffName { get; set; }
    }

}
