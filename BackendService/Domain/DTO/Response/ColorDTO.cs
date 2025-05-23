using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Response
{
    public class ColorDTO
    {
        public int ColorId { get; set; }

        public string ColorName { get; set; } = null!;

        public string? ColorCode { get; set; }

        public DateTime? CreatedDate { get; set; }
    }
    public class CreateMultipleColor
    {
        public List<CreateColorDTO> Colorssd { get; set; }
    }
    public class CreateColorDTO
    {
        public string ColorName { get; set; } = null!;

        public string? ColorCode { get; set; }

        public DateTime? CreatedDate { get; set; }
    }
}
