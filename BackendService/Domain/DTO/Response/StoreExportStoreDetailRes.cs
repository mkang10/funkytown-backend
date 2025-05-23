using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Response
{
    public class StoreExportStoreDetailDto
    {
        public int DispatchId { get; set; }

        public int DispatchStoreDetailId { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public int? ActualQuantity { get; set; }
        public int AllocatedQuantity { get; set; }
        public string? Status { get; set; }
        public string? Comments { get; set; }
        public int? StaffDetailId { get; set; }
        public int? DispatchDetailId { get; set; }
        public int? HandleBy { get; set; }
        public string? HandleByName { get; set; }
        
        public string? ProductName { get; set; }

        public string? SizeName { get; set; }

        public string? ColorName { get; set; }


    }

}
