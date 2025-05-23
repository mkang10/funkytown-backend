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
    public class DispatchHandler
    {
        private readonly IDispatchRepos _dispatchRepos;
        private readonly IAuditLogRepository _auditRepos;
        private readonly IMapper _mapper;

        public DispatchHandler(IDispatchRepos dispatchRepos, IAuditLogRepository auditRepos, IMapper mapper)
        {
            _dispatchRepos = dispatchRepos;
            _auditRepos = auditRepos;
            _mapper = mapper;
        }

        public async Task<DispatchGet> GetJSONDispatchByIdHandler(int id)
        {
            try
            {
                var data = await _dispatchRepos.GetJSONDispatchById(id);
                if (data == null)
                {
                    throw new Exception("Dispatch does not exsist!");
                }
                var dataModel = _mapper.Map<JSONDispatchDTO>(data);

                var audit = await _auditRepos.GetAuditLogsByTableAndRecordIdAsync(TableEnumEXE.Dispatch.ToString(), id.ToString()) ;
                var dataAudit = _mapper.Map<List<AuditLogRes>>(audit);

                return new DispatchGet
                {
                    JSONDispatchGet = dataModel,
                    AuditLogs = dataAudit,
                };
            }
            catch (Exception ex)
            {
                throw new Exception("An error occur: " + ex.Message);
            }
        }

        public async Task<JSONStoreExportStoreDetailByIdHandlerDTO> GetJSONStoreExportStoreDetailByIdHandler(int id)
        {
            var data = await _dispatchRepos.GetStoreExportStoreDetailById(id);
            if (data == null)
            {
                throw new Exception("Dispatch Store Detail does not exsist!");
            }
            var dataModel = _mapper.Map<JSONStoreExportStoreDetailByIdHandlerDTO>(data);
            var audit = await _auditRepos.GetAuditLogsByTableAndRecordIdAsync(TableEnumEXE.StoreExportStoreDetail.ToString(), id.ToString());
            dataModel.AuditLogs = _mapper.Map<List<AuditLogRes>>(audit);
            return dataModel;
        }
    }
}
