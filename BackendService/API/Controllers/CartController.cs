
using Application.UseCases;
using Domain.DTO.Request;
using Domain.DTO.Response;
using Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.Controllers
{
    [ApiController]
    [Route("api/cart")]
    public class CartController : ControllerBase
    {
        private readonly GetShoppingCartHandler _cartHandler;

        public CartController(GetShoppingCartHandler cartHandler)
        {
            _cartHandler = cartHandler;
        }

        // Lấy giỏ hàng của tài khoản
        [HttpGet("{accountId}")]
        public async Task<ActionResult<ResponseDTO<List<CartItemResponse>>>> GetCart(int accountId)
        {
            var response = await _cartHandler.Handle(accountId);
            if (!response.Status)
            {
                return NotFound(response);
            }
            return Ok(response);
        }

        // Thêm sản phẩm vào giỏ hàng
        [HttpPost("{accountId}/add")]
        public async Task<ActionResult<ResponseDTO<bool>>> AddCartItem(int accountId, [FromBody] AddToCartRequest cartItemRequest)
        {
            var response = await _cartHandler.AddCartItem(accountId, cartItemRequest);
            if (!response.Status)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        // Xóa/bớt sản phẩm khỏi giỏ hàng theo productVariantId
        [HttpDelete("{accountId}/remove/{productVariantId}")]
        public async Task<ActionResult<ResponseDTO<bool>>> RemoveCartItem(int accountId, int productVariantId)
        {
            var response = await _cartHandler.RemoveCartItem(accountId, productVariantId);

            if (!response.Status)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        // Xóa toàn bộ giỏ hàng của tài khoản
        [HttpDelete("{accountId}/clear")]
        public async Task<ActionResult<ResponseDTO<bool>>> ClearCart(int accountId)
        {
            var response = await _cartHandler.ClearCart(accountId);

            if (!response.Status)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        // Đồng bộ giỏ hàng (trên Redis) sang Database
        [HttpPost("sync-cart/{accountId}")]
        public async Task<IActionResult> SyncCartToDatabase(int accountId)
        {
            await _cartHandler.SyncCartToDatabase(accountId);
            return Ok(new { message = "Giỏ hàng đã được đồng bộ sang Database." });
        }

        [HttpPost("{accountId}/clear-after-order")]
        public async Task<IActionResult> ClearCartAfterOrder(int accountId, [FromBody] List<int> selectedProductVariantIds)
        {
            if (selectedProductVariantIds == null || !selectedProductVariantIds.Any())
            {
                return BadRequest(new { message = "Danh sách sản phẩm cần xóa không hợp lệ." });
            }

            await _cartHandler.ClearCartAfterOrderAsync(accountId, selectedProductVariantIds);
            return Ok(new { message = "Các sản phẩm được chọn đã được xóa khỏi giỏ hàng." });
        }

        // Cập nhật số lượng sản phẩm trong giỏ hàng (tăng/giảm 1)
        [HttpPost("{accountId}/change-quantity")]
        public async Task<ActionResult<ResponseDTO<bool>>> ChangeCartItemQuantity(int accountId, [FromBody] ChangeCartItemQuantityRequest request)
        {
            var response = await _cartHandler.ChangeCartItemQuantity(accountId, request);

            if (!response.Status)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

    }
}
