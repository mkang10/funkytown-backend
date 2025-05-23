using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Request
{
    public class CreateLogGHNDTO
    {
        public string TableName { get; set; } = null!;

        public string RecordId { get; set; } = null!;

        public string Operation { get; set; } = null!;

        public DateTime ChangeDate { get; set; }

        public int? ChangedBy { get; set; }

        public string? ChangeData { get; set; }

        public string? Comment { get; set; }
    }
}
