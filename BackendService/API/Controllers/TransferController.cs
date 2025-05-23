using Application.UseCases;
using Domain.DTO.Request;
using Domain.DTO.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Services; // ReportService
using System.IO;
using System.IO.Compression;
using Application.DTO.Response;

[ApiController]
[Route("api/[controller]")]
public class TransferController : ControllerBase
{
    private readonly TransferHandler _transferHandler;
    private readonly GetAllTransferHandler _getHandler;
    private readonly ReportService _reportService;

    public TransferController(
        TransferHandler transferHandler,
        GetAllTransferHandler getHandler,
        ReportService reportService)
    {
        _transferHandler = transferHandler;
        _getHandler = getHandler;
        _reportService = reportService;
    }

    /// <summary>
    /// Tạo đơn chuyển hàng full flow và trả về file zip chứa 3 biên bản:
    /// - Phiếu Chuyển Hàng
    /// - Phiếu Xuất Kho
    /// - Phiếu Nhập Kho
    /// </summary>
    /// <param name="request">Thông tin tạo chuyển hàng full flow</param>
    /// <returns>File zip chứa 3 biên bản dưới dạng file .docx</returns>
    [HttpPost("create-transfer-fullflow")]
    public async Task<IActionResult> CreateTransferFullFlow([FromBody] CreateTransferFullFlowDto request)
    {
        try
        {
            if (request == null || request.TransferDetails == null || !request.TransferDetails.Any())
                return BadRequest(new ResponseDTO<object>(null, false, "Dữ liệu chuyển hàng không hợp lệ!"));

            // Gọi full flow để tạo đơn chuyển hàng
            var fullFlowResponse = await _transferHandler.CreateTransferFullFlowAsync(request);
            if (!fullFlowResponse.Status)
                return BadRequest(fullFlowResponse);

            // Giả sử fullFlowResponse.Data chứa thông tin Transfer, và có thuộc tính TransferId
            int transferId = fullFlowResponse.Data.TransferOrderId;

            // Lấy thông tin chi tiết của các đơn liên quan
            var transfer = await _transferHandler.GetTransferByIdAsync(transferId);
            var export = await _transferHandler.GetExportByTransferIdAsync(transferId);
            var import = await _transferHandler.GetImportByTransferIdAsync(transferId);

            if (transfer == null || export == null || import == null)
                return NotFound(new ResponseDTO<object>(null, false, "Không tìm thấy dữ liệu liên quan cho biên bản."));

            // Tạo 3 file Word từ dữ liệu đã lấy
            byte[] transferSlipBytes = _reportService.GenerateTransferSlip(transfer);
            byte[] exportSlipBytes = _reportService.GenerateExportSlip(export);
            byte[] importSlipBytes = _reportService.GenerateImportSlip(import);

            // Tạo file zip chứa 3 file Word
            using (var zipStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
                {
                    // Phiếu chuyển hàng
                    var transferEntry = archive.CreateEntry($"PhieuChuyenHang_{transfer.TransferOrderId}.docx", CompressionLevel.Fastest);
                    using (var entryStream = transferEntry.Open())
                    {
                        entryStream.Write(transferSlipBytes, 0, transferSlipBytes.Length);
                    }

                    // Phiếu xuất kho
                    var exportEntry = archive.CreateEntry($"PhieuXuatKho_{export.ReferenceNumber}.docx", CompressionLevel.Fastest);
                    using (var entryStream = exportEntry.Open())
                    {
                        entryStream.Write(exportSlipBytes, 0, exportSlipBytes.Length);
                    }

                    // Phiếu nhập kho
                    var importEntry = archive.CreateEntry($"PhieuNhapKho_{import.ReferenceNumber}.docx", CompressionLevel.Fastest);
                    using (var entryStream = importEntry.Open())
                    {
                        entryStream.Write(importSlipBytes, 0, importSlipBytes.Length);
                    }
                }
                zipStream.Seek(0, SeekOrigin.Begin);
                byte[] zipBytes = zipStream.ToArray();
                return File(zipBytes, "application/zip", $"BienBan_{transfer.TransferOrderId}.zip");
            }
        }
        catch (System.ArgumentException argEx)
        {
            return BadRequest(new ResponseDTO<object>(null, false, argEx.Message));
        }
        catch (System.UnauthorizedAccessException uaEx)
        {
            return Unauthorized(new ResponseDTO<object>(null, false, uaEx.Message));
        }
        catch (System.Exception ex)
        {
            return StatusCode(500, new ResponseDTO<object>(null, false, $"Lỗi server: {ex.Message}"));
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
           [FromQuery] int page = 1,
           [FromQuery] int pageSize = 10,
           [FromQuery] string? filter = null,
           CancellationToken cancellationToken = default)
    {
        var result = await _getHandler.HandleAsync(page, pageSize, filter, cancellationToken);
        if (!result.Status)
            return BadRequest(result);

        return Ok(result);
    }
    [HttpGet("transfer/{id}")]
    public async Task<IActionResult> GetJSONTransferByIdController(int id)
    {
        try
        {
            var result = await _transferHandler.GetJSONTransferById(id);
            if (result == null)
            {
                var notFoundResponse = new MessageRespondDTO<JSONTransferDispatchImportGet>(null, false, "Transfer not found!");
                return NotFound(notFoundResponse);
            }
            var successResponse = new MessageRespondDTO<JSONTransferDispatchImportGet>(result, true, "Success!");
            return Ok(successResponse);
        }
        catch (Exception ex)
        {
            var errorResponse = new MessageRespondDTO<object>(null, false, "An error occurred: " + ex.Message);
            return BadRequest(errorResponse);
        }
    }
}
