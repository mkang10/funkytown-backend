
using Application.UseCases;
using Domain.DTO.Request;
using Domain.DTO.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/promotion")]
    public class PromotionController : ControllerBase
    {
        private readonly CreatePromotionHandler _createPromotionHandler;
        private readonly UpdatePromotionHandler _updatePromotionHandler;
        private readonly DeletePromotionHandler _deletePromotionHandler;
        private readonly GetAllPromotionsHandler _getAllPromotionsHandler;
        public PromotionController(CreatePromotionHandler createPromotionHandler,
                                   UpdatePromotionHandler updatePromotionHandler, 
                                   DeletePromotionHandler deletePromotionHandler,
                                   GetAllPromotionsHandler getAllPromotionsHandler)
        {
            _createPromotionHandler = createPromotionHandler;
            _updatePromotionHandler = updatePromotionHandler;
            _deletePromotionHandler = deletePromotionHandler;
            _getAllPromotionsHandler = getAllPromotionsHandler;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreatePromotion([FromBody] CreatePromotionRequest request)
        {
            var promotionId = await _createPromotionHandler.Handle(request);
            return Created("", new ResponseDTO<int>(promotionId, true, "Khuyến mãi được tạo thành công!"));
        }

        [HttpPut("update/{promotionId}")]
        public async Task<IActionResult> UpdatePromotion(int promotionId, [FromBody] UpdatePromotionRequest request)
        {
            var success = await _updatePromotionHandler.UpdatePromotionAsync(promotionId, request);
            return success
                ? Ok(new ResponseDTO(true, "Cập nhật khuyến mãi thành công!"))
                : NotFound(new ResponseDTO(false, "Khuyến mãi không tồn tại!"));
        }

        [HttpDelete("delete/{promotionId}")]
        public async Task<IActionResult> DeletePromotion(int promotionId)
        {
            var success = await _deletePromotionHandler.Handle(promotionId);
            return success ? Ok(new ResponseDTO(true, "Xóa khuyến mãi thành công!")) : NotFound(new ResponseDTO(false, "Khuyến mãi không tồn tại!"));
        }
        [HttpGet("list")]
        public async Task<IActionResult> GetAllPromotions([FromQuery] string? status)
        {
            var promotions = await _getAllPromotionsHandler.Handle(status);
            return Ok(new ResponseDTO<List<PromotionResponse>>(promotions, true, "Lấy danh sách khuyến mãi thành công!"));
        }

        [HttpPut("update-status/{promotionId}")]
        public async Task<IActionResult> UpdatePromotionStatus(int promotionId, [FromBody] UpdatePromotionStatusRequest request)
        {
            var success = await _updatePromotionHandler.UpdatePromotionStatusAsync(promotionId, request);
            return success
                ? Ok(new ResponseDTO(true, "Cập nhật trạng thái khuyến mãi thành công!"))
                : NotFound(new ResponseDTO(false, "Khuyến mãi không tồn tại!"));
        }
    }

}
