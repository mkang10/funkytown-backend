using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Response
{
    public class WarehouseDTO
    {
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public string? WarehouseDescription { get; set; }
        public string Location { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? ImagePath { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string WarehouseType { get; set; }

        public List<ImportStoreDetailDTO> ImportStoreDetails { get; set; }
    }


    public class ImportStoreDetailDTO
    {
        public int? ActualReceivedQuantity { get; set; }
        public int AllocatedQuantity { get; set; }
        public string? Status { get; set; }
        public string? Comments { get; set; }
        public int? StaffDetailId { get; set; }
        public int ImportDetailId { get; set; }
        public int ImportStoreId { get; set; }
        public int? WarehouseId { get; set; }
        public int? HandleBy { get; set; }

        // Chỉ include những thông tin cần thiết của HandleByNavigation
        public ShopManagerDetailDTO? HandleByNavigation { get; set; }
    }
    public class ShopManagerDetailDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

}
