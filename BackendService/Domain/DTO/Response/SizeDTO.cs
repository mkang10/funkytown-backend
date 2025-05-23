using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Response
{
    public class SizeDTO
    {
        public int SizeId { get; set; }

        public string SizeName { get; set; } = null!;

        public string? SizeDescription { get; set; }

        public DateTime? CreatedDate { get; set; }
    }
    public class CreateMultipleSize
    {
        public List<CreateSizeDTO> Colorssd { get; set; }
    }

    public class CreateSizeDTO
    {
        public string SizeName { get; set; } = null!;

        public string? SizeDescription { get; set; }

        public DateTime? CreatedDate { get; set; }
    }
}
