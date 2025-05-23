using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Request
{
    public class UpdateStoreDetailDto
    {
        public int StoreDetailId { get; set; }
        public int ActualReceivedQuantity { get; set; }
        public string? Comment { get; set; }
    }
}
