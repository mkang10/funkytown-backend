
using Application.UseCases;
using Domain.DTO.Request;
using Domain.DTO.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
	[ApiController]
	[Route("api/favorites")]
	public class FavoriteProductController : ControllerBase
	{
		private readonly AddFavoriteHandler _addHandler;
		private readonly RemoveFavoriteHandler _removeHandler;
		private readonly GetFavoriteProductsHandler _getHandler;

		public FavoriteProductController(
			AddFavoriteHandler addHandler,
			RemoveFavoriteHandler removeHandler,
			GetFavoriteProductsHandler getHandler)
		{
			_addHandler = addHandler;
			_removeHandler = removeHandler;
			_getHandler = getHandler;
		}

		[HttpGet("{accountId}")]
		public async Task<IActionResult> GetFavorites(int accountId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
		{
			var result = await _getHandler.Handle(accountId, page, pageSize);

			return Ok(new ResponseDTO<List<ProductListResponse>>(result, true, "Lấy danh sách sản phẩm yêu thích thành công."));
		}

		[HttpPost("{accountId}/{productId}")]
		public async Task<IActionResult> Add(int accountId, int productId)
		{
			var result = await _addHandler.HandleAsync(new FavoriteRequest
			{
				AccountId = accountId,
				ProductId = productId
			});

			if (result == null)
				return NotFound(new ResponseDTO(false, "Không tìm thấy sản phẩm."));

			return Ok(new ResponseDTO<ProductListResponse>(result, true, "Đã thêm sản phẩm vào yêu thích."));
		}

		[HttpDelete("{accountId}/{productId}")]
		public async Task<IActionResult> Remove(int accountId, int productId)
		{
			var result = await _removeHandler.HandleAsync(new FavoriteRequest
			{
				AccountId = accountId,
				ProductId = productId
			});

			if (result == null)
				return NotFound(new ResponseDTO(false, "Không tìm thấy sản phẩm."));

			return Ok(new ResponseDTO<ProductListResponse>(result, true, "Đã xoá sản phẩm khỏi yêu thích."));
		}
	}

}
