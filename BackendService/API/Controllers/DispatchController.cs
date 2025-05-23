using Application.Enum;
using Application.Interfaces;
using Application.UseCases;
using Domain.DTO.Request;
using Domain.DTO.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DispatchController : ControllerBase
    {
        private readonly DispatchHandler _service;
        private readonly DispatchDoneHandler _dispatchDoneHandler;
        private readonly GetAllDispatchHandler _getAllDispatchHandler;
        private readonly AssignStaffHandler _assignStaff;
        private readonly GetAllExportByStaffHandler _getDispatchByStaff;

        public DispatchController(GetAllExportByStaffHandler getDispatchByStaff, AssignStaffHandler assignStaff, DispatchDoneHandler dispatchDoneHandler, GetAllDispatchHandler getAllDispatchHandler, DispatchHandler service)
        {
            _service = service;
            _dispatchDoneHandler = dispatchDoneHandler;
            _getAllDispatchHandler = getAllDispatchHandler;
            _assignStaff = assignStaff;
            _getDispatchByStaff = getDispatchByStaff; 
        }
        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllDispatch([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] DispatchFilterDto filter = null)
        {
            var result = await _getAllDispatchHandler.HandleAsync(page, pageSize, filter);
            return Ok(result);
        }

        [HttpPut("{dispatchId}/assign-staff")]
        public async Task<IActionResult> AssignStaffDetail(int dispatchId, [FromQuery] int staffDetailId)
        {
            var response = await _assignStaff.AssignStaffDispatchAccountAsync(dispatchId, staffDetailId);
            if (!response.Status)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }



        [HttpPost("{dispatchid}/done")]
        public async Task<IActionResult> ProcessImportDone(int dispatchid, int staffId, [FromBody] List<UpdateStoreDetailDto> confirmations)
        {
            if (confirmations == null || confirmations.Count == 0)
            {
                var errorResponse = new ResponseDTO<string>(null, false, "Danh sách xác nhận không được để trống");
                return BadRequest(errorResponse);
            }

            try
            {
                await _dispatchDoneHandler.ProcessDispatchDoneAsync(dispatchid, staffId, confirmations);
                var response = new ResponseDTO<string>("Cập nhật Done thành công", true, "Success");
                return Ok(response);
            }
            catch (ArgumentException argEx)
            {
                var errorResponse = new ResponseDTO<string>(null, false, argEx.Message);
                return BadRequest(errorResponse);
            }
            catch (InvalidOperationException invOpEx)
            {
                var errorResponse = new ResponseDTO<string>(null, false, invOpEx.Message);
                return Conflict(errorResponse);
            }
            catch (Exception ex)
            {
                var errorResponse = new ResponseDTO<string>(null, false, "Đã có lỗi xảy ra, vui lòng thử lại sau");
                return StatusCode(500, errorResponse);
            }
        }

        //[HttpGet("by-staff")]
        //public async Task<IActionResult> GetExportDetailsByStaffDetail([FromQuery] int page = 1,
        //[FromQuery] int pageSize = 10,
        //[FromQuery] StoreExportStoreDetailFilterDto filter = null)
        //{
        //    var paged = await _getDispatchByStaff.HandleAsync(page, pageSize, filter);
        //    var response = new ResponseDTO<PaginatedResponseDTO<ExportDetailDto>>(
        //        paged, true, "Lấy danh sách export store details thành công");
        //    return Ok(response);
        //}

        [HttpGet("by-staff")]
        public async Task<IActionResult> GetStoreDetailsByStaffDetail([FromQuery] StoreExportStoreDetailFilterDtO filter)
        {
            try
            {
                var response = await _getDispatchByStaff.GetStoreExportByStaffDetailAsync(filter);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO<object>(null, false, $"Server error: {ex.Message}"));
            }
        }

        [HttpGet("dispatch/{id}")]
        public async Task<IActionResult> GetDispatchByIdController(int id)
        {
            try
            {
                var result = await _service.GetJSONDispatchByIdHandler(id);
                if (result == null)
                {
                    var notFoundResponse = new MessageRespondDTO<DispatchGet>(null, false, "Dispatch not found!");
                    return NotFound(notFoundResponse);
                }
                var successResponse = new MessageRespondDTO<DispatchGet>(result, true, "Success!");
                return Ok(successResponse);
            }
            catch (Exception ex)
            {
                var errorResponse = new MessageRespondDTO<object>(null, false, "An error occurred: " + ex.Message);
                return BadRequest(errorResponse);
            }
        }

        [HttpGet("export-store/{id}")]
        public async Task<IActionResult> GetJSONStoreExportStoreDetailByIdController(int id)
        {
            try
            {
                var result = await _service.GetJSONStoreExportStoreDetailByIdHandler(id);
                if (result == null)
                {
                    var notFoundResponse = new MessageRespondDTO<JSONStoreExportStoreDetailByIdHandlerDTO>(null, false, "Dispatch not found!");
                    return NotFound(notFoundResponse);
                }
                var successResponse = new MessageRespondDTO<JSONStoreExportStoreDetailByIdHandlerDTO>(result, true, "Success!");
                return Ok(successResponse);
            }
            catch (Exception ex)
            {
                var errorResponse = new MessageRespondDTO<object>(null, false, "An error occurred: " + ex.Message);
                return BadRequest(errorResponse);
            }
        }
    }
}
