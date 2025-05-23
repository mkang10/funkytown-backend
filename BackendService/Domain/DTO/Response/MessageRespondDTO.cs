using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Response
{
    public class MessageRespondDTO<T>
    {
        public T Data { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }
        public MessageRespondDTO(T data, bool status, string message)
        {
            Data = data;
            Status = status;
            Message = message;
        }
        public class MessageResponseButNoData
        {
            public string Message { get; set; }
            public bool Success { get; set; }
        }
    }

}
