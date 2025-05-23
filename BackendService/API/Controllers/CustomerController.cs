
using Application.UseCases;
using Domain.DTO.Request;
using Domain.DTO.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/customer")]
    public class CustomerController : ControllerBase
    {
        private readonly EditProfileHandler _editProfileHandler;
        private readonly GetCustomerProfileHandler _getCustomerProfileService;
        private readonly InteractionHandler _interactionHandler;
        private readonly SuggestProductsHandler _suggestProductsHandler;
        private readonly PreferredStyleHandler _preferredStyleHandler;
        public CustomerController(EditProfileHandler editProfileHandler,
                                  GetCustomerProfileHandler getCustomerProfileService,
                                  InteractionHandler interactionHandler, 
                                  SuggestProductsHandler suggestProductsHandler,
                                  PreferredStyleHandler preferredStyleHandler)
        {
            _editProfileHandler = editProfileHandler;
            _getCustomerProfileService = getCustomerProfileService;
            _interactionHandler = interactionHandler;
            _suggestProductsHandler = suggestProductsHandler;
            _preferredStyleHandler = preferredStyleHandler;
        }

        [HttpGet("profile/{accountId}")]
        public async Task<ActionResult<ResponseDTO<CustomerProfileResponse>>> GetCustomerProfile(int accountId)
        {
            var result = await _getCustomerProfileService.GetCustomerProfile(accountId);
            if (result == null)
            {
                return NotFound(new ResponseDTO<CustomerProfileResponse>(null, false, "Customer not found"));
            }
            return Ok(new ResponseDTO<CustomerProfileResponse>(result, true, "Customer profile retrieved successfully."));
        }

        [HttpPut("edit-profile/{accountId}")]
        public async Task<ActionResult<ResponseDTO<EditProfileResponse>>> EditProfile(
                                                                            int accountId,
                                                                            [FromForm] EditProfileRequest request) 
        {
            var result = await _editProfileHandler.EditProfile(accountId, request);
            if (!result.Success)
            {
                return NotFound(new ResponseDTO<EditProfileResponse>(result, false, "Edit profile failed"));
            }
            return Ok(new ResponseDTO<EditProfileResponse>(result, true, "Edit profile successful"));
        }
        [HttpPost("products/interactions")]
        public async Task<IActionResult> RecordProductInteraction([FromBody] ProductInteractionRequest request)
        {
            await _interactionHandler.HandleAsync(request.AccountId, request.ProductId);

            var response = new ResponseDTO(
                status: true,
                message: "Ghi nhận tương tác sản phẩm thành công."
            );

            return Ok(response);
        }

        [HttpGet("suggestions")]
        public async Task<IActionResult> GetSuggestedProducts([FromQuery] int accountId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (accountId <= 0)
            {
                return BadRequest(new ResponseDTO(false, "AccountId không hợp lệ."));
            }

            var suggestedProducts = await _suggestProductsHandler.HandleAsync(accountId, page, pageSize);

            return Ok(new ResponseDTO<List<SuggestedProductResponse>>(suggestedProducts, true, "Lấy sản phẩm gợi ý thành công."));
        }

        [HttpGet("preferred-styles/{accountId}")]
        public async Task<IActionResult> GetPreferredStyles(int accountId)
        {
            var styles = await _preferredStyleHandler.GetPreferredStylesByAccountIdAsync(accountId);

            if (styles == null || !styles.Any())
            {
                return NotFound(new ResponseDTO<List<StyleResponse>>(null, false, "Không tìm thấy style yêu thích nào."));
            }

            return Ok(new ResponseDTO<List<StyleResponse>>(styles, true, "Lấy danh sách style yêu thích thành công."));
        }

        [HttpPut("preferred-styles/{accountId}")]
        public async Task<IActionResult> UpdatePreferredStyles(int accountId, [FromBody] UpdatePreferredStylesRequest request)
        {
            var response = await _preferredStyleHandler.UpdatePreferredStylesAsync(accountId, request.StyleIds);

            if (!response.Status)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

    }
}
