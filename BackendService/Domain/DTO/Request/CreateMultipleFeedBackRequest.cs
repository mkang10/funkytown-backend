using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Request
{
    public class CreateMultipleFeedBackRequest
    {
        public List<CreateFeedBackArrayRequestDTO> Feedbacks { get; set; }
    }

}
