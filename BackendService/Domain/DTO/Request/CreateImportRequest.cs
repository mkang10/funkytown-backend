using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Request
{
    public class CreateImportDto
    {
        // Thông tin của đơn Import
        public int CreatedBy { get; set; }

        public string? ReferenceNumber { get; set; }

        public List<CreateImportDetailDto> ImportDetails { get; set; } = new();
    }


    public class CreateImportDetailDto
    {
        public int ProductVariantId { get; set; }
        public int Quantity { get; set; }
        public decimal CostPrice { get; set; }

        public List<CreateStoreDetailDto> StoreDetails { get; set; } = new();
    }

    public class CreateStoreDetailDto
    {
        public int WareHouseId { get; set; }
        public int AllocatedQuantity { get; set; }

        public int? HandleBy { get; set; }

    }
}
