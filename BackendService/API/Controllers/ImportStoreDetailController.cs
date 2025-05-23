using Application.DTO.Response;
using Application.Enum;
using Application.Interfaces;
using Application.UseCases;
using Domain.DTO.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImportStoreDetailController : ControllerBase
    {
        private readonly ImportStoreDetailHandler _service;

        public ImportStoreDetailController(ImportStoreDetailHandler service)
        {
            _service = service;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetImportStoreDetailByIdController(int id)
        {
            try
            {
                var result = await _service.GetJSONImportStoreDetailByIdHandler(id);
                if (result == null)
                {
                    var notFoundResponse = new MessageRespondDTO<JSONImportStoreDetailDTO>(null, false, "Dispatch not found!");
                    return NotFound(notFoundResponse);
                }
                var successResponse = new MessageRespondDTO<JSONImportStoreDetailDTO>(result, true, "Success!");
                return Ok(successResponse);
            }
            catch (Exception ex)
            {
                var errorResponse = new MessageRespondDTO<object>(null, false, "An error occurred: " + ex.Message);
                return BadRequest(errorResponse);
            }
        }
    }
}
