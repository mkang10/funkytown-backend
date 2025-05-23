using Application.Enum;
using Application.UseCases;
using Domain.Commons;
using Domain.DTO.Response;
using Domain.Enum;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SizeController : ControllerBase
    {
        private readonly SizeHandler _sizeHandler;

        public SizeController(SizeHandler sizeHandler)
        {
            _sizeHandler = sizeHandler;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllSize([FromQuery] PaginationParameter paginationParameter)
        {
            try
            {
                var result = await _sizeHandler.GetAllSize(paginationParameter);

                if (result == null)
                {
                    var notFoundResponse = new ResponseDTO<object>(null, false, StatusSuccess.Wrong.ToString());
                    return NotFound(notFoundResponse);
                }
                else
                {
                    var metadata = new
                    {
                        result.TotalCount,
                        result.PageSize,
                        result.CurrentPage,
                        result.TotalPages,
                        result.HasNext,
                        result.HasPrevious
                    };

                    Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(metadata));
                }
                var successResponse = new MessageRespondDTO<object>(result, true, StatusSuccess.Success.ToString());
                return Ok(successResponse);
            }
            catch (Exception ex)
            {
                var errorResponse = new MessageRespondDTO<object>(null, false, "An error occurred: " + ex.Message);
                return BadRequest(errorResponse);
            }
        }
        [HttpGet("{name}")]
        public async Task<IActionResult> GetBySizeNameController(string name)
        {
            try
            {
                var result = await _sizeHandler.GetBySizeName(name);
                if (result == null)
                {
                    var notFoundResponse = new MessageRespondDTO<object>(null, false, "Data not found.");
                    return NotFound(notFoundResponse);
                }
                var successResponse = new MessageRespondDTO<object>(result, true, "successfully.");
                return Ok(successResponse);
            }
            catch (Exception ex)
            {
                var errorResponse = new MessageRespondDTO<object>(null, false, "An error occurred: " + ex.Message);
                return BadRequest(errorResponse);
            }
        }
        [HttpPost("create")]
        public async Task<IActionResult> CreateSizeController([FromBody] CreateMultipleSize data)
        {
            try
            {
                var dataCreate = await _sizeHandler.CreateSize(data.Colorssd);
                if (dataCreate == null)
                {
                    return BadRequest(new MessageRespondDTO<object>(null, false, StatusSuccess.Wrong.ToString()));
                }
                return Ok(new MessageRespondDTO<List<SizeDTO>>(dataCreate, true, StatusSuccess.Success.ToString()));
            }
            catch (Exception ex)
            {
                return BadRequest(new MessageRespondDTO<object>(null, false, ex.Message));
            }
        }
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteSizeByNameController(string id)
        {
            try
            {
                var result = await _sizeHandler.DeleteSize(id);
                if (result)
                {
                    return Ok(new MessageRespondDTO<object>(null, true, StatusSuccess.Success.ToString()));

                }
                return BadRequest(new MessageRespondDTO<object>(null, false, StatusSuccess.Wrong.ToString()));

            }
            catch (Exception ex)
            {
                var errorResponse = new MessageRespondDTO<object>(null, false, "An error occurred: " + ex.Message);
                return BadRequest(errorResponse);
            }
        }

        [HttpPut("update/")]
        public async Task<IActionResult> UpdateController( CreateSizeDTO data)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new MessageRespondDTO<object>(null, false, StatusSuccess.Wrong.ToString()));
                }

                bool isUpdated = await _sizeHandler.UpdateSize( data);

                if (isUpdated)
                {
                    return Ok(new MessageRespondDTO<object>(null, true, StatusSuccess.Success.ToString()));
                }
                else
                {
                    return NotFound(new MessageRespondDTO<object>(null, false, "Wrong Id to update!"));
                }
            }
            catch (Exception ex)
            {
                var errorResponse = new MessageRespondDTO<object>(null, false, ex.Message);
                return BadRequest(errorResponse);
            }
        }
    }
}
