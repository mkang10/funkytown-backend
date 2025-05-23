using Application.Enum;
using Application.UseCases;
using Domain.Commons;
using Domain.DTO.Response;
using Domain.Entities;
using Domain.Enum;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Reflection.Metadata;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly CategoryHandler _sizeHandler;

        public CategoryController(CategoryHandler sizeHandler)
        {
            _sizeHandler = sizeHandler;
        }

        //[HttpGet]
        //public async Task<IActionResult> GetAll()
        //{
        //    try
        //    {
        //        var result = await _handler.GetAllAsync();
        //        var response = new ResponseDTO<IEnumerable<Category>>(result, true, "Lấy danh sách thành công");

        //        return Ok(response);
        //    }
        //    catch (Exception ex)
        //    {
        //        var response = new ResponseDTO<string>(null, false, $"Lỗi: {ex.Message}");
        //        return StatusCode(500, response);
        //    }
        //}

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PaginationParameter paginationParameter)
        {
            try
            {
                var result = await _sizeHandler.GetAllCategoryHandler(paginationParameter);

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
        public async Task<IActionResult> GetByNameController(string name)
        {
            try
            {
                var result = await _sizeHandler.GetByName(name);
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
        public async Task<IActionResult> CreateController([FromBody] CreateMultipleCategory data)
        {
            try
            {
                var dataCreate = await _sizeHandler.CreateOrUpdateCategory(data.Cate);
                if (dataCreate == null)
                {
                    return BadRequest(new MessageRespondDTO<object>(null, false, StatusSuccess.Wrong.ToString()));
                }
                return Ok(new MessageRespondDTO<List<CategoryDTO>>(dataCreate, true, StatusSuccess.Success.ToString()));
            }
            catch (Exception ex)
            {
                return BadRequest(new MessageRespondDTO<object>(null, false, ex.Message));
            }
        }
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteController(int id)
        {
            try
            {
                var result = await _sizeHandler.DeleteColor(id);
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

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateController(int id, CreateCategoryDTO data)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new MessageRespondDTO<object>(null, false, StatusSuccess.Wrong.ToString()));
                }

                bool isUpdated = await _sizeHandler.UpdateColor(id, data);

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
