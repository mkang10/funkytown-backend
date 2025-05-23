
using Application.UseCases;
using Domain.DTO.Request;
using Domain.DTO.Response;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/shippingaddresses")]
    [ApiController]
    public class ShippingAddressController : ControllerBase
    {
        private readonly IShippingAddressRepository _shippingAddressRepository;
        private readonly ILogger<ShippingAddressController> _logger;
        private readonly ShippingCostHandler _shippingCostHandler;
        private readonly GetShippingAddressHandler _shippingAddressHandler;
        
        public ShippingAddressController(IShippingAddressRepository shippingAddressRepository,
                                         ILogger<ShippingAddressController> logger,
                                         ShippingCostHandler shippingCostHandler,
                                         GetShippingAddressHandler shippingAddressHandler)
        {
            _shippingAddressRepository = shippingAddressRepository;
            _logger = logger;
            _shippingCostHandler = shippingCostHandler;
            _shippingAddressHandler = shippingAddressHandler;
        }

        [HttpPost]
        public async Task<ActionResult<ResponseDTO<ShippingAddressResponse>>> CreateShippingAddress(
                                                                        [FromBody] CreateShippingAddressRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(ms => ms.Value?.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                    );

                var errorResponse = new ResponseDTO<Dictionary<string, string[]>>(errors, false, "Dữ liệu không hợp lệ");
                return BadRequest(errorResponse);
            }

            var response = await _shippingAddressHandler.CreateShippingAddressHandler(request);

            return CreatedAtAction(
                nameof(GetShippingAddressById),
                new { id = response.Data.AddressId },
                response
            );
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<ResponseDTO<ShippingAddress>>> GetShippingAddressById(int id)
        {
            var address = await _shippingAddressRepository.GetByIdAsync(id);

            if (address == null)
            {
                return Ok(new ResponseDTO<ShippingAddress>(
                    null,
                    true,
                    "Địa chỉ không tồn tại"
                ));
            }

            return Ok(new ResponseDTO<ShippingAddress>(
                address,
                true,
                "Lấy địa chỉ thành công"
            ));
        }



        [HttpGet("account/{accountId}")]
        public async Task<ActionResult<ResponseDTO<List<ShippingAddress>>>> GetShippingAddressesByAccountId(int accountId)
        {
            var addresses = await _shippingAddressRepository.GetShippingAddressesByAccountIdAsync(accountId);

            if (addresses == null || !addresses.Any())
            {
                // Trả về danh sách rỗng, status true, message rõ ràng
                return Ok(new ResponseDTO<List<ShippingAddress>>(
                    new List<ShippingAddress>(),
                    true,
                    "Chưa có địa chỉ nào"
                ));
            }

            return Ok(new ResponseDTO<List<ShippingAddress>>(
                addresses,
                true,
                "Lấy danh sách địa chỉ thành công"
            ));
        }



        [HttpGet("cost")]
        public IActionResult GetShippingCost([FromQuery] string city, [FromQuery] string district)
        {
            var shippingCost = _shippingCostHandler.CalculateShippingCost(city, district);
            return Ok(new ResponseDTO<decimal>(shippingCost, true, "Shipping cost calculated successfully"));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ResponseDTO<ShippingAddressResponse>>> UpdateShippingAddress(
                                                                    int id, [FromBody] UpdateShippingAddressRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(ms => ms.Value?.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                    );

                return BadRequest(new ResponseDTO<Dictionary<string, string[]>>(errors, false, "Dữ liệu không hợp lệ"));
            }

            var result = await _shippingAddressHandler.UpdateShippingAddressHandler(id, request);
            return Ok(result);
        }


        
        [HttpDelete("{id}")]
        public async Task<ActionResult<ResponseDTO>> DeleteShippingAddress(int id)
        {
            var result = await _shippingAddressHandler.DeleteShippingAddressHandler(id);

            return Ok(new ResponseDTO(true, result.Message));
        }


    }


}
