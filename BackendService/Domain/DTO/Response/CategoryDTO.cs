using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Response
{
    public class CategoryDTO
    {
        public int CategoryId { get; set; }

        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public int? ParentCategoryId { get; set; }

        public bool? IsActive { get; set; }

        public int? DisplayOrder { get; set; }
    }
    public class CreateMultipleCategory
    {
        public List<CreateCategoryDTO> Cate { get; set; }
    }
    public class CreateCategoryDTO
    {
        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public int? ParentCategoryId { get; set; }

        public bool? IsActive { get; set; }

        public int? DisplayOrder { get; set; }
    }
}
