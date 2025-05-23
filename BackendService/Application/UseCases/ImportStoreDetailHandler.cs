using Application.Enum;
using AutoMapper;
using Domain.DTO.Response;
using Domain.Enum;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class ImportStoreDetailHandler
    {
        private readonly IImportRepos _importRepos;
        private readonly IAuditLogRepository _auditRepos;
        private readonly IMapper _mapper;

        public ImportStoreDetailHandler(IImportRepos importRepos, IAuditLogRepository auditRepos, IMapper mapper)
        {
            _importRepos = importRepos;
            _auditRepos = auditRepos;
            _mapper = mapper;
        }

        public async Task<JSONImportStoreDetailDTO> GetJSONImportStoreDetailByIdHandler(int id)
        {
            var data = await _importRepos.GetImportStoreDetail(id);
            if (data == null)
            {
                throw new Exception("Import Store Detail does not exsist!");
            }
            var dataModel =  _mapper.Map<JSONImportStoreDetailDTO>(data);
            var audit = await _auditRepos.GetAuditLogsByTableAndRecordIdAsync(TableEnumEXE.ImportStoreDetail.ToString(), id.ToString());
            dataModel.AuditLogs = _mapper.Map<List<AuditLogRes>>(audit);
            return dataModel;
        }

    }
}
