using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Response
{
    public class ProductDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string? ImagePath { get; set; }
        public string? Origin { get; set; }
        public string? Model { get; set; }
        public string? Occasion { get; set; }
        public string? Style { get; set; }
        public string? Material { get; set; }
        public string? Status { get; set; }
        public List<Images> Image { get; set; } = new(); // ✅ thêm dòng 


        public class Images
        {
            public int ProductImageId { get; set; }


            public string ImagePath { get; set; } = null!;

            public bool IsMain { get; set; }


        }
    }
}