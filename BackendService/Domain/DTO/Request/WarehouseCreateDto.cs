using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Request
{
    public class WarehouseCreateDto
    {
        public string WarehouseName { get; set; } = null!;
        public string? WarehouseDescription { get; set; }
        public string Location { get; set; } = null!;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string WarehouseType { get; set; } = null!;
        public int? ShopManagerId { get; set; }

        public bool? IsOwnerWarehouse { get; set; }

        

        // Image upload
        public IFormFile? ImageFile { get; set; }
    }
}
