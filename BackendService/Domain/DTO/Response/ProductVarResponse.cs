//using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Response
{

    public class ProductVariantManageResponse
    {
        public int VariantId { get; set; }
        public string ProductName { get; set; } = null!;
        public string? SizeName { get; set; }
        public string? ColorName { get; set; }
        public string? MainImagePath { get; set; }
    }




}
