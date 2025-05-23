using AutoMapper;
using Domain.DTO.Response;
using Domain.Interfaces;

public class GetImportDetailHandler
{
    private readonly IImportRepos _importRepos;
    private readonly IAuditLogRepository _auditLogRepos;
    private readonly IMapper _mapper;

    public GetImportDetailHandler(IImportRepos importRepos, IAuditLogRepository auditLogRepos, IMapper mapper)
    {
        _importRepos = importRepos;
        _auditLogRepos = auditLogRepos;
        _mapper = mapper;
    }

    public async Task<InventoryImportDetailDto> GetInventoryDetailAsync(int importId)
    {
        // Lấy thông tin Import bao gồm các detail, store detail, v.v.
        var import = await _importRepos.GetImportByIdAsync(importId);
        if (import == null)
        {
            throw new Exception("Không tìm thấy phiếu nhập kho có Id: " + importId);
        }

        // Mapping dữ liệu Import sang DTO
        var dto = _mapper.Map<InventoryImportDetailDto>(import);

        // Lấy danh sách AuditLog cho bảng Import với RecordId bằng importId
        var auditLogs = await _auditLogRepos.GetAuditLogsByTableAndRecordIdAsync("Import", importId.ToString());

        // Ánh xạ sang DTO của AuditLog và gán vào InventoryImportDetailDto
        dto.AuditLogs = _mapper.Map<List<AuditLogRes>>(auditLogs);

        return dto;
    }
}
