using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Request
{
    public class EditProductVariantDto
    {
        public int VariantId { get; set; }
        public decimal Price { get; set; }
        public string? Status { get; set; }
        public int? MaxStocks { get; set; }

        public IFormFile? ImageFile { get; set; }            // new property for upload
    }

    public class ColorDto
    {
        public int ColorId { get; set; }
        public string ColorName { get; set; } = null!;
        public string? ColorCode { get; set; }
    }

    public class SizeDto
    {
        public int SizeId { get; set; }
        public string SizeName { get; set; } = null!;
        public string? SizeDescription { get; set; }
    }

}
