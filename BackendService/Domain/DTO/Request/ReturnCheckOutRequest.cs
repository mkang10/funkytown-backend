using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Request
{
    public class ReturnCheckOutRequest
    {
        public int OrderId { get; set; }
        public int AccountId { get; set; }
        public List<SelectedReturnItemRequest> SelectedItems { get; set; } = new();
    }

}
