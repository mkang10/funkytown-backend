using Application.UseCases;
using Domain.DTO.Request;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/checkout")]
    public class CheckOutController : ControllerBase
    {
        private readonly CheckOutHandler _checkOutHandler;

        public CheckOutController(CheckOutHandler checkOutHandler)
        {
            _checkOutHandler = checkOutHandler;
        }

        [HttpPost]
        public async Task<IActionResult> CheckOut([FromBody] CheckOutRequest request)
        {
            if (request == null || request.SelectedProductVariantIds == null || !request.SelectedProductVariantIds.Any())
            {
                return BadRequest("Danh sách sản phẩm không hợp lệ.");
            }

            var checkOutResponse = await _checkOutHandler.Handle(request);
            if (checkOutResponse == null)
            {
                return BadRequest("Không thể thực hiện checkout. Vui lòng kiểm tra lại thông tin sản phẩm hoặc địa chỉ giao hàng.");
            }

            return Ok(checkOutResponse);
        }
    }
}
