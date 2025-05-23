using Application.UseCases;
using Domain.DTO.Request;
using Domain.DTO.Response;
using Domain.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;

namespace API.Controllers
{
    [ApiController]
    [Route("api/warehouses")]
    public class WarehouseController : ControllerBase
    { 
        private readonly GetWarehouseByIdHandler _getStoreByIdHandler;
        private readonly GetWareHouseStockByVariantHandler _stockHandler;
        private readonly UpdateStockAfterOrderHandler _updateStockHandler;
        private readonly GetStockQuantityHandler _getStockQuantityHandler;
        private readonly CreateWarehouseHandler _createHandler;
        private readonly GetWareHouseIdHandler _handler;
        private readonly GetWarehouseStockHandler _Gethandler;


        public WarehouseController(GetWarehouseByIdHandler getStoreByIdHandler,
                                   GetWareHouseStockByVariantHandler stockHandler,
                                   GetStockQuantityHandler getStockQuantityHandler,
                                   UpdateStockAfterOrderHandler updateStockHandler,
                                   CreateWarehouseHandler createHandler ,
                                   GetWareHouseIdHandler handler,
                                   GetWarehouseStockHandler Gethandler
            )
                {
                    _getStoreByIdHandler = getStoreByIdHandler;
                    _stockHandler = stockHandler;
                    _getStockQuantityHandler = getStockQuantityHandler;
                    _updateStockHandler = updateStockHandler;
            _createHandler = createHandler;
            _handler = handler;
            _Gethandler = Gethandler;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var result = await _Gethandler.HandleAsync(page, pageSize, cancellationToken);
            if (!result.Status)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ResponseDTO<Domain.DTOs.WarehouseStockDto>>> Get(int id)
        {
            var dto = await _handler.GetByIdAsync(id);
            if (dto == null)
            {
                return NotFound(new ResponseDTO<Domain.DTOs.WarehouseStockDto>(null!, false, "Warehouse stock not found"));
            }

            return Ok(new ResponseDTO<Domain.DTOs.WarehouseStockDto>(dto, true, "Fetch warehouse stock successfully"));
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ResponseDTO<int>>> Create([FromForm] WarehouseCreateDto dto)
        {
            var id = await _createHandler.HandleAsync(dto);
            return Ok(new ResponseDTO<int>(id, true, "Warehouse created successfully"));
        }

        [HttpGet("warehouse/{warehouseId}")]
        public async Task<ActionResult<PaginatedResponseDTO<GetWareHouseStockRes>>> GetByWarehouse(
            int warehouseId,
            [FromQuery] string? productName,
            [FromQuery] string? sizeName,
            [FromQuery] string? colorName,
            [FromQuery] int? stockQuantity,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _handler.GetByWarehouseIdAsync(
                warehouseId,
                productName,
                sizeName,
                colorName,
                stockQuantity,
                page,
                pageSize);
            return Ok(result);
        }

        /// <summary>
        /// Lấy tổng tồn kho của một variant trên toàn hệ thống.
        /// </summary>
        /// 

        [HttpGet("variant/{variantId}/total-stock")]
        public async Task<ActionResult<ResponseDTO<int>>> GetTotalStock(int variantId)
        {
            int totalStock = await _stockHandler.HandleTotalStockAsync(variantId);
            var response = new ResponseDTO<int>(totalStock, true, "Lấy tổng tồn kho thành công");
            return Ok(response);
        }

        /// <summary>
        /// Lấy phân rã tồn kho của một variant theo từng cửa hàng.
        /// </summary>
        //[HttpGet("variant/{variantId}/stock-breakdown")]
        //public async Task<ActionResult<ResponseDTO<object>>> GetStockBreakdown(int variantId)
        //{
        //    // Giả sử breakdown là một object, có thể thay thế bằng kiểu cụ thể nếu có
        //    var breakdown = await _stockHandler.HandleStockBreakdownAsync(variantId);
        //    var response = new ResponseDTO<object>(breakdown, true, "Lấy phân rã tồn kho thành công");
        //    return Ok(response);
        //}
        [HttpGet("{warehouseId}/stock/{productVariantId}")]
        public async Task<IActionResult> GetStockQuantity(int warehouseId, int productVariantId)
        {
            var result = await _getStockQuantityHandler.HandleAsync(warehouseId, productVariantId);

            var response = new ResponseDTO<StockQuantityResponse>(
                result,
                result != null,
                result != null ? "Lấy số lượng tồn kho thành công." : "Không tìm thấy dữ liệu tồn kho."
            );

            if (result == null)
                return NotFound(response); // 404 Not Found nếu không có dữ liệu.

            return Ok(response); // 200 OK nếu có dữ liệu.
        }
        [HttpPost("update-after-order")]
        public async Task<ActionResult<ResponseDTO>> UpdateStockAfterOrder([FromBody] StockUpdateRequest request)
        {
            var response = await _updateStockHandler.HandleAsync(request);

            if (!response.Success)
            {
                return BadRequest(new ResponseDTO(false, response.Message));
            }

            return Ok(new ResponseDTO(true, "Cập nhật tồn kho thành công."));
        }

        [HttpPost("restore-after-cancel")]
        public async Task<ActionResult<ResponseDTO>> RestoreStockAfterCancel([FromBody] StockUpdateRequest request)
        {
            var response = await _updateStockHandler.HandleRestoreStockAsync(request);

            if (!response.Success)
            {
                return BadRequest(new ResponseDTO(false, response.Message));
            }

            return Ok(new ResponseDTO(true, "Khôi phục tồn kho sau khi huỷ đơn thành công."));
        }

    }
}
