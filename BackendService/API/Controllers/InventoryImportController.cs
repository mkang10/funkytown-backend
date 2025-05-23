using Application.Services;
using Application.UseCases;
using Domain.DTO.Request;
using Domain.DTO.Response;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryImportController : ControllerBase
    {
        private readonly ApproveHandler _appHandler;
        private readonly RejectHandler _reHandler;
        private readonly GetAllImportHandler _getHandler;
        private readonly GetImportDetailHandler _getDetailHandler;
        private readonly CreateImportHandler _createImportHandler;
        private readonly GetWareHouseHandler _getWareHouseHandler;
        private readonly IImportRepos _importRepos;
        private readonly ReportService _reportService;

        private readonly CreateImportTransferHandler _createhandler;
        private readonly GetImportHandler _gethandler;
        private readonly GetAllProductHandler _getProductVar;
        private readonly GetAllStaffHandler _getAllStaff;
        private readonly AssignStaffHandler _assignStaff;
        private readonly ImportDoneHandler _importDoneHandler;
        private readonly ImportIncompletedHandler _importIncompletedHandler;
        private readonly ImportShortageHandler _importShortageHandler;

        private readonly GetAllStaffImportHandler _getAllStaffImport;
        private readonly GetAllImportStoreHandler _getAllImportStore;

        public InventoryImportController(IImportRepos importRepos, ReportService reportService, GetWareHouseHandler getWareHouseHandler, CreateImportHandler createImportHandler, GetImportDetailHandler getDetailHandler, ApproveHandler appHandler, RejectHandler reHandler, GetAllImportHandler getHandler,  GetAllImportStoreHandler getAllImportStore, ImportShortageHandler importShortageHandler, GetAllStaffImportHandler getAllStaffImport, ImportDoneHandler importDoneHandler, ImportIncompletedHandler importIncompletedHandler, AssignStaffHandler assignStaff, GetAllStaffHandler getAllStaff, CreateImportTransferHandler createhandler, GetImportHandler Gethandler, GetAllProductHandler getProductVar)
        {
            _appHandler = appHandler;
            _reHandler = reHandler;
            _getHandler = getHandler;
            _getDetailHandler = getDetailHandler;
            _createImportHandler = createImportHandler;
            _getWareHouseHandler = getWareHouseHandler;
            _importRepos = importRepos;
            _createhandler = createhandler;
            _gethandler = Gethandler;
            _getProductVar = getProductVar;
            _getAllStaff = getAllStaff;
            _assignStaff = assignStaff;
            _importDoneHandler = importDoneHandler;
            _importIncompletedHandler = importIncompletedHandler;
            _importShortageHandler = importShortageHandler;
            _getAllStaffImport = getAllStaffImport;
            _getAllImportStore = getAllImportStore;
            _reportService = reportService;
        }

        [HttpGet("product")]
        public async Task<IActionResult> GetAllProductVariants([FromQuery] int page = 1, [FromQuery] int pageSize = 10, string search = null)
        {
            try
            {
                var pagedVariants = await _getProductVar.GetAllProductVariantsAsync(page, pageSize, search);
                var response = new ResponseDTO<PaginatedResponseDTO<ProductVariantResponseDto>>(pagedVariants, true, "Lấy danh sách Product Variant thành công.");
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new ResponseDTO<string>(null, false, $"Có lỗi xảy ra: {ex.Message}");
                return StatusCode(500, errorResponse);
            }
        }

        [HttpGet("names")]
        public async Task<IActionResult> GetStaffNames(int warehouseId)
        {
            var response = await _getAllStaff.GetAllStaffNamesAsync(warehouseId);
            if (!response.Status)
            {
                return NotFound(response);
            }
            return Ok(response);
        }

        [HttpPut("{importId}/assign-staff")]
        public async Task<IActionResult> AssignStaffDetail(int importId, [FromQuery] int staffDetailId)
        {
            var response = await _assignStaff.AssignStaffAccountAsync(importId, staffDetailId);
            if (!response.Status)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllImports([FromQuery] ImportFilterDto filter)
        {
            try
            {
                var response = await _gethandler.GetAllImportsAsync(filter);
                return Ok(response);
            }
            catch (KeyNotFoundException knfEx)
            {
                // Không tìm thấy dữ liệu
                var errorResponse = new ResponseDTO<object>(null, false, knfEx.Message);
                return NotFound(errorResponse);
            }
            catch (ArgumentException argEx)
            {
                // Tham số không hợp lệ
                var errorResponse = new ResponseDTO<object>(null, false, argEx.Message);
                return BadRequest(errorResponse);
            }
            catch (UnauthorizedAccessException uaEx)
            {
                // Người dùng không được phép truy cập
                var errorResponse = new ResponseDTO<object>(null, false, uaEx.Message);
                return Unauthorized(errorResponse);
            }
            catch (Exception ex)
            {
                // Các lỗi không mong đợi khác
                var errorResponse = new ResponseDTO<object>(null, false, $"Internal Server Error: {ex.Message}");
                return StatusCode(500, errorResponse);
            }
        }

        




        [HttpPost("{importId}/done")]
        public async Task<IActionResult> ProcessImportDone(int importId, int staffId, [FromBody] List<UpdateStoreDetailDto> confirmations)
        {
            if (confirmations == null || confirmations.Count == 0)
            {
                var errorResponse = new ResponseDTO<string>(null, false, "Danh sách xác nhận không được để trống");
                return BadRequest(errorResponse);
            }

            try
            {
                await _importDoneHandler.ProcessImportDoneAsync(importId, staffId, confirmations);
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

        /// <summary>
        /// API endpoint xử lý cập nhật ImportStoreDetail thành "Incompleted".
        /// Chỉ cho phép khi Import có trạng thái Processing.
        /// </summary>
        [HttpPost("{importId}/incompleted")]
        public async Task<IActionResult> ProcessImportIncompleted(int importId, int staffId, [FromBody] List<UpdateStoreDetailDto> confirmations)
        {
            if (confirmations == null || confirmations.Count == 0)
            {
                var errorResponse = new ResponseDTO<string>(null, false, "Danh sách xác nhận không được để trống");
                return BadRequest(errorResponse);
            }

            try
            {
                await _importIncompletedHandler.ProcessImportIncompletedAsync(importId, staffId, confirmations);
                var response = new ResponseDTO<string>("Cập nhật Incompleted thành công", true, "Success");
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
        [HttpGet("assign-staff")]
        public async Task<IActionResult> GetImportStoreDetail([FromQuery] ImportStoreDetailFilterDtO filter)
        {
            try
            {
                var response = await _getAllImportStore.GetStoreExportByStaffDetailAsync(filter);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO<object>(null, false, $"Server error: {ex.Message}"));
            }
        }

        [HttpGet("by-staff")]
        public async Task<IActionResult> GetStoreDetailsByStaffDetail([FromQuery] ImportStoreDetailFilterDto filter)
        {
            try
            {
                var response = await _getAllStaffImport.GetStoreDetailsByStaffDetailAsync(filter);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO<object>(null, false, $"Server error: {ex.Message}"));
            }
        }

        [HttpPost("shortage")]
        public async Task<IActionResult> ProcessImportShortage(
            [FromQuery] int importId,
            [FromQuery] int staffId,
            [FromBody] List<UpdateStoreDetailDto> confirmations)
        {
            try
            {
                await _importShortageHandler.ImportIncompletedAsync(importId, staffId, confirmations);
                var response = new ResponseDTO<string>(
                    data: "Cập nhật tồn kho cho đơn nhập hàng thiếu thành công",
                    status: true,
                    message: "Success"
                );
                return Ok(response);
            }
            catch (Exception ex)
            {
                var response = new ResponseDTO<string>(
                    data: null,
                    status: false,
                    message: ex.Message
                );
                return BadRequest(response);
            }
        }

        [HttpPost("{importId}/approve")]
        public async Task<IActionResult> ApproveImport(
        int importId,
        [FromBody] ApproveRejectRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join("; ", ModelState.Values
                                                    .SelectMany(v => v.Errors)
                                                    .Select(e => e.ErrorMessage));
                return BadRequest(new ResponseDTO<string>(null, false, $"Validation errors: {errors}"));
            }

            try
            {
                // 1. Thực hiện approve
                await _appHandler.ApproveImportAsync(importId, request.ChangedBy, request.Comments);

                // 2. Load lại entity Import với chi tiết để làm báo cáo
                //    (Giả sử GetByIdAsyncWithDetails bao gồm ImportDetails + ImportStoreDetails)
                var importEntity = await _importRepos.GetByIdAsyncWithDetails(importId);
                if (importEntity == null)
                    return NotFound(new ResponseDTO<string>(null, false, "Không tìm thấy đơn nhập đã approve."));

                // 3. Gọi ReportService để sinh file báo cáo Import Slip
                byte[] slipBytes = _reportService.GenerateImportSlip(importEntity);

                // 4. Trả về file để client download
                string fileName = $"PhieuNhap_{importEntity.ReferenceNumber}_{DateTime.Now:yyyyMMddHHmmss}.docx";
                return File(
                    slipBytes,
                    "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    fileName
                );
            }
            catch (ArgumentException argEx)
            {
                return BadRequest(new ResponseDTO<string>(null, false, argEx.Message));
            }
            catch (UnauthorizedAccessException uaEx)
            {
                return Unauthorized(new ResponseDTO<string>(null, false, uaEx.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO<string>(null, false, $"Lỗi server: {ex.Message}"));
            }
        }

            // Endpoint reject: POST api/InventoryImport/{importId}/reject
            [HttpPost("{importId}/reject")]
        public async Task<IActionResult> RejectImport(int importId, [FromBody] ApproveRejectRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors)
                                                                  .Select(e => e.ErrorMessage));
                var response = new ResponseDTO<string>(null, false, $"Validation errors: {errors}");
                return BadRequest(response);
            }

            try
            {
                await _reHandler.RejectImportAsync(importId, request.ChangedBy, request.Comments);
                var response = new ResponseDTO<string>("", true, "Inventory import rejected successfully.");
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new ResponseDTO<string>(null, false, $"Error: {ex.Message}");
                return StatusCode(500, errorResponse);
            }
        }

        [HttpGet("filter")]
        public async Task<IActionResult> GetInventoryImports([FromQuery] InventoryImportFilterDto filter)
        {
            try
            {
                var pagedResult = await _getHandler.GetInventoryImportsAsync(filter);
                var response = new ResponseDTO<PagedResult<InventoryImportResponseDto>>(
                    pagedResult,
                    true,
                    "Lấy danh sách Inventory Import thành công."
                );
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new ResponseDTO<string>(null, false, $"Có lỗi xảy ra: {ex.Message}");
                return StatusCode(500, errorResponse);
            }
        }

        [HttpGet("{importId}")]
        public async Task<IActionResult> GetInventoryDetail(int importId)
        {
            try
            {
                var inventoryDetail = await _getDetailHandler.GetInventoryDetailAsync(importId);
                var response = new ResponseDTO<InventoryImportDetailDto>(inventoryDetail, true, "Lấy dữ liệu thành công.");
                return Ok(response);
            }
            catch (Exception ex)
            {
                var response = new ResponseDTO<InventoryImportDetailDto>(null, false, ex.Message);
                return BadRequest(response);
            }
        }
        [HttpPost("createtransfer-from-excel")]
        public async Task<IActionResult> CreateImportFromExcel(
    [FromForm] IFormFile file,
    [FromForm] int warehouseId,
    [FromForm] int createdBy)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new ResponseDTO<object>(null, false, "Vui lòng upload file Excel."));

                // Gọi service để import từ Excel
                var response = await _createhandler.CreateTRansferImportFromExcelAsync(file, warehouseId, createdBy);
                // Ở đây vẫn giữ response.Status = true ngay cả khi importEntity.Status = "Rejected"
                if (!response.Status)
                    return BadRequest(response);

                // Lấy lại entity import vừa tạo
                var importEntity = await _importRepos.GetByIdAsync(response.Data.ImportId);
                if (importEntity == null)
                    return NotFound(new ResponseDTO<object>(null, false, "Không tìm thấy đơn nhập sau khi tạo."));

                // Nếu đơn bị reject thì chỉ trả về response DTO chứ không sinh file
                if (string.Equals(importEntity.Status, "Rejected", StringComparison.OrdinalIgnoreCase))
                {
                    // Trả về thành công (200 OK) với thông báo, nhưng không có file
                    return Ok(new ResponseDTO<object>(
                        null,
                        true,
                        "Đơn nhập đã bị từ chối, bạn không thể tải biên bản nhập kho, vì các kho lân cận đều không đủ hàng"
                    ));
                }

                // Chỉ đến đây nếu Status != "Rejected"
                byte[] slipFile = _reportService.GenerateImportSlip(importEntity);
                string fileName = $"PhieuNhap_{importEntity.ReferenceNumber}_{DateTime.Now:yyyyMMddHHmmss}.docx";

                return File(
                    slipFile,
                    "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    fileName
                );
            }
            catch (ArgumentException argEx)
            {
                return BadRequest(new ResponseDTO<object>(null, false, argEx.Message));
            }
            catch (UnauthorizedAccessException uaEx)
            {
                return Unauthorized(new ResponseDTO<object>(null, false, uaEx.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO<object>(null, false, $"Lỗi server: {ex.Message}"));
            }
        }


        [HttpPost("createtransfer")]
        public async Task<IActionResult> CreateImport([FromBody] TransImportDto request)
        {
            try
            {
                // 1. Validate request
                if (request == null || request.ImportDetails == null || !request.ImportDetails.Any())
                    return BadRequest(new ResponseDTO<object>(null, false, "Dữ liệu import không hợp lệ!"));

                // 2. Gọi handler (Status=true dù đã reject)
                var response = await _createhandler.CreateTransferImportAsync(request);

                // Nếu handler có report lỗi nghiêm trọng (ví dụ validate đầu vào, lỗi DB…), vẫn BadRequest
                if (!response.Status)
                    return BadRequest(response);

                // 3. Lấy import entity để đọc trạng thái thật
                var importEntity = await _importRepos.GetByIdAsync(response.Data.ImportId);
                if (importEntity == null)
                    return NotFound(new ResponseDTO<object>(null, false, "Không tìm thấy đơn nhập sau khi tạo."));

                // 4. Nếu import bị REJECTED → chỉ trả JSON, giữ status=true nhưng không trả file
                if (string.Equals(importEntity.Status, "Rejected", StringComparison.OrdinalIgnoreCase))
                {
                    var payload = new { ImportId = importEntity.ImportId };
                    return Ok(new ResponseDTO<object>(
                        payload,
                        true,
                        "Đơn của bạn đã bị từ chối vì sản phẩm không đủ để điều phối"
                    ));
                }

                // 5. Ngược lại (Approved) → sinh file và trả về
                byte[] slipFile = _reportService.GenerateImportSlip(importEntity);
                string fileName = $"PhieuNhap_{importEntity.ReferenceNumber}_{DateTime.Now:yyyyMMddHHmmss}.docx";

                return File(
                    slipFile,
                    "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    fileName
                );
            }
            catch (ArgumentException argEx)
            {
                return BadRequest(new ResponseDTO<object>(null, false, argEx.Message));
            }
            catch (UnauthorizedAccessException uaEx)
            {
                return Unauthorized(new ResponseDTO<object>(null, false, uaEx.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO<object>(null, false, $"Lỗi server: {ex.Message}"));
            }
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateImport([FromBody] PurchaseImportCreateDto request)
        {
            try
            {
                //if (request == null || request. == null || !request.ImportDetails.Any())
                //    return BadRequest(new ResponseDTO<object>(null, false, "Dữ liệu import không hợp lệ!"));

                var response = await _createImportHandler.CreatePurchaseImportAsync(request);
                if (!response.Status)
                    return BadRequest(response);

                // Sau khi tạo thành công, load lại Import từ repository (theo ImportId được trả về trong response)
                var importEntity = await _importRepos.GetByIdAsync(response.Data.ImportId);
                if (importEntity == null)
                    return NotFound(new ResponseDTO<object>(null, false, "Không tìm thấy đơn nhập sau khi tạo."));

                // Gọi ReportService để tạo file biên bản nhập hàng
                byte[] slipFile = _reportService.GenerateImportSlip(importEntity);
                string fileName = $"PhieuNhap_{importEntity.ReferenceNumber}_{DateTime.Now:yyyyMMddHHmmss}.docx";
                return File(slipFile, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", fileName);
            }
            catch (ArgumentException argEx)
            {
                return BadRequest(new ResponseDTO<object>(null, false, argEx.Message));
            }
            catch (UnauthorizedAccessException uaEx)
            {
                return Unauthorized(new ResponseDTO<object>(null, false, uaEx.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO<object>(null, false, $"Lỗi server: {ex.Message}"));
            }
        }

        [HttpPost("create-from-excel")]
 
        public async Task<IActionResult> CreateImportFromExcel(
        [FromForm] IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new ResponseDTO<object>(null, false, "Vui lòng upload file Excel."));


                // Gọi service để import từ Excel
                var response = await _createImportHandler.CreatePurchaseImportFromExcelAsync(file, 18);
                if (!response.Status)
                    return BadRequest(response);

                // Lấy lại entity import vừa tạo
                var importEntity = await _importRepos.GetByIdAsync(response.Data.ImportId);
                if (importEntity == null)
                    return NotFound(new ResponseDTO<object>(null, false, "Không tìm thấy đơn nhập sau khi tạo."));

                // Sinh file biên bản nhập kho
                byte[] slipFile = _reportService.GenerateImportSlip(importEntity);
                string fileName = $"PhieuNhap_{importEntity.ReferenceNumber}_{DateTime.Now:yyyyMMddHHmmss}.docx";

                return File(
                    slipFile,
                    "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    fileName
                );
            }
            catch (ArgumentException argEx)
            {
                return BadRequest(new ResponseDTO<object>(null, false, argEx.Message));
            }
            catch (UnauthorizedAccessException uaEx)
            {
                return Unauthorized(new ResponseDTO<object>(null, false, uaEx.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO<object>(null, false, $"Lỗi server: {ex.Message}"));
            }
        }

        /// <summary>
        /// Tạo đơn nhập hàng bổ sung và trả về file Word (biên bản nhập hàng) dựa trên đơn bổ sung vừa tạo.
        /// </summary>
        [HttpPost("create-supplement")]
        public async Task<IActionResult> CreateSupplementImport([FromBody] SupplementImportRequestDto request)
        {
            try
            {
                if (request == null || request.ImportDetails == null || !request.ImportDetails.Any())
                    return BadRequest(new ResponseDTO<object>(null, false, "Dữ liệu import không hợp lệ!"));
                if (request.OriginalImportId <= 0)
                    return BadRequest(new ResponseDTO<object>(null, false, "OriginalImportId không hợp lệ!"));

                var response = await _createImportHandler.CreateSupplementImportAsync(request);
                if (!response.Status)
                    return BadRequest(response);

                // Sau khi tạo đơn bổ sung, load lại đơn bổ sung và đơn cũ đầy đủ dữ liệu
                var supplementImportEntity = await _importRepos.GetByIdAsync(response.Data.ImportData.ImportId);
                var oldImportEntity = await _importRepos.GetByIdAsync(response.Data.ImportData.OriginalImportId.Value);
                if (supplementImportEntity == null || oldImportEntity == null)
                    return NotFound(new ResponseDTO<object>(null, false, "Không tìm thấy dữ liệu đơn nhập khi tạo báo cáo."));

                // Tạo báo cáo nhập bổ sung
                byte[] reportFileBytes = _reportService.GenerateImportSupplementSlip(supplementImportEntity, oldImportEntity);
                string fileName = $"PhieuNhapBoSung_{supplementImportEntity.ReferenceNumber}_{DateTime.Now:yyyyMMddHHmmss}.docx";
                return File(reportFileBytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", fileName);
            }
            catch (ArgumentException argEx)
            {
                return BadRequest(new ResponseDTO<object>(null, false, argEx.Message));
            }
            catch (UnauthorizedAccessException uaEx)
            {
                return Unauthorized(new ResponseDTO<object>(null, false, uaEx.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO<object>(null, false, $"Lỗi server: {ex.Message}"));
            }
        }

        [HttpGet("WareHouse")]
        public async Task<IActionResult> GetAllWarehouses([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var pagedWareHouse = await _getWareHouseHandler.GetAllWareHouse(page, pageSize);
                var response = new ResponseDTO<PaginatedResponseDTO<Warehouse>>(pagedWareHouse, true, "Lấy danh sách warehouse thành công.");
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new ResponseDTO<string>(null, false, $"Có lỗi xảy ra: {ex.Message}");
                return StatusCode(500, errorResponse);
            }
        }

    }
}
