using Application.Interfaces;
using Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/images")]
    [ApiController]
    public class ImageController : ControllerBase
    {
        private readonly ICloudinaryService _cloudinaryService;

        public ImageController(ICloudinaryService cloudinaryService)
        {
            _cloudinaryService = cloudinaryService;
        }

        //[HttpPost("upload")]
        //[Consumes("multipart/form-data")]

        //public async Task<IActionResult> UploadImage([FromForm] IFormFile file)
        //{
        //    if (file == null || file.Length == 0)
        //    {
        //        return BadRequest(new { Message = "Vui lòng chọn một file hợp lệ." });
        //    }

        //    var imageUrl = await _cloudinaryService.UploadMediaAsync(file);
        //    return Ok(new { Url = imageUrl });
        //}

        [HttpDelete("delete/{publicId}")]
        public async Task<IActionResult> DeleteImage(string publicId)
        {
            var result = await _cloudinaryService.DeleteMediaAsync(publicId);
            return result ? Ok("Xóa thành công") : BadRequest("Không thể xóa ảnh");
        }
    }

}
