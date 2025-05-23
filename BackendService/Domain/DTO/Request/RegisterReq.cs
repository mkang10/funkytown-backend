using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Request
{
	
    public class RegisterReq
    {
        public string Username { get; set; }
        public bool IsActive { get; set; }

        public string Password { get; set; }
        public string Email { get; set; }
    }

    
}
