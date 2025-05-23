using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Response
{
	public class TokenResponse
	{
        public string? Token { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public bool Success => Errors == null || !Errors.Any();
    }
}
