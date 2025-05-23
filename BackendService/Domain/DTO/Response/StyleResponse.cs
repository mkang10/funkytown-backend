using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Response
{
    public class StyleResponse
    {
        public int StyleId { get; set; }
        public string StyleName { get; set; } = string.Empty;
        public bool IsSelected { get; set; } = false;
    }

}
