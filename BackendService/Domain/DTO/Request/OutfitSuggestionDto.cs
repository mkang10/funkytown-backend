using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Request
{
    public class OutfitSuggestionDto
    {
        public ProductVariant? Top { get; set; }
        public ProductVariant? Bottom { get; set; }
        public ProductVariant? Shoes { get; set; }
        public ProductVariant? Accessory { get; set; }
    }
}
