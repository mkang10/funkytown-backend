using Application.UseCases;
using Domain.DTO.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly DashboardHandler _dashboard;
        public DashboardController(DashboardHandler dashboard)
        {
            _dashboard = dashboard;
        }

        /// <summary>
        /// Get dashboard data, optionally filtering by status (e.g. "Pending", "Approved")
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ResponseDTO<DashboardDto>>> Get([FromQuery] string? status)
        {
            var data = await _dashboard.GetDashboardAsync(status);
            var response = new ResponseDTO<DashboardDto>(data, true, "Fetched dashboard successfully");
            return Ok(response);

        }
    }
}
