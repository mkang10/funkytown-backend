using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Response
{
    public class ProductVariantDetailDTO
    {
        public int VariantId { get; set; }
        public string ProductName { get; set; } = null!;
        public string? SizeName { get; set; }
        public string? ColorName { get; set; }
        public decimal Price { get; set; }
        public string? ImagePath { get; set; }
        public string? Sku { get; set; }
        public string? Barcode { get; set; }
        public decimal? Weight { get; set; }
        public string? Status { get; set; }

        public int? MaxStock { get; set; }

    }

}
