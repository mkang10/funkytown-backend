using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Request
{
    public class UpdatePreferredStylesRequest
    {
        public List<int> StyleIds { get; set; } = new();
    }

}
