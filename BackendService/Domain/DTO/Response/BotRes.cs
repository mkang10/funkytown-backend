using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Response
{


    public class ChatCompletionResponse
    {
        public List<Choice>? Choices { get; set; }

        public class Choice
        {
            public Message? Message { get; set; }

            // Nếu LLM chọn gọi function, thông tin sẽ nằm ở đây
            public FunctionCall? FunctionCall { get; set; }
        }

        public class Message
        {
            public string? Role { get; set; }
            public string? Content { get; set; }

            // Một số LLM có thể để function_call ở trong Message (nếu trả về vậy)
            public FunctionCall? FunctionCall { get; set; }
        }

        public class FunctionCall
        {
            public string? Name { get; set; }

            // Thường là JSON string, ví dụ: { "heightCm": 170, "weightKg": 60 }
            public string? Arguments { get; set; }
        }
    }



}
