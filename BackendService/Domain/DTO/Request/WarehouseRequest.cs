using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Request
{
    public class WarehouseRequest
    {
        public string WarehouseName { get; set; } = null!;

        public string? WarehouseDescription { get; set; }

        public string Location { get; set; } = null!;

        public DateTime? CreatedDate { get; set; }

        public string? ImagePath { get; set; }

        public string? Email { get; set; }

        public string? Phone { get; set; }

        public string WarehouseType { get; set; } = null!;

    }
}
