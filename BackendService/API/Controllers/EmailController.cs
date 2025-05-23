
using Application.UseCases;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using Infrastructure.HelperServices;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailController : ControllerBase
    {
        private readonly EmailHandler _emailService;

        public EmailController(EmailHandler emailService)
        {
            _emailService = emailService;
        }

        [HttpGet("send-mail")]
        public async Task<IActionResult> SendMail(int id)
        {
            await _emailService.InvoiceForEmail(id);
            return Ok("Đã gửi email!");
        }
    }
}