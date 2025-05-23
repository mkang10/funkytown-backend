using Application.Enums;
using Domain.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class AuditLogHandler
    {
        private readonly IAuditLogRepository _auditLogRepository;

        public AuditLogHandler(IAuditLogRepository auditLogRepository)
        {
            _auditLogRepository = auditLogRepository;
        }

        public async Task LogOrderActionAsync(
                                            int orderId,
                                            AuditOperation operation,
                                            object? changeDetails,
                                            int changedBy,
                                            string comment)
        {
            string? changeData = changeDetails != null
                ? JsonConvert.SerializeObject(changeDetails)
                : null;

            await _auditLogRepository.AddAuditLogAsync(
                tableName: "Orders",
                recordId: orderId.ToString(),
                operation: operation.ToString(),
                changedBy: changedBy,
                changeData: changeData,
                comment: comment
            );
        }
        public async Task LogReturnOrderActionAsync(
                                                int returnOrderId,
                                                AuditOperation operation,
                                                object? changeDetails,
                                                int changedBy,
                                                string comment)
        {
            string? changeData = changeDetails != null
                ? JsonConvert.SerializeObject(changeDetails)
                : null;

            await _auditLogRepository.AddAuditLogAsync(
                tableName: "ReturnOrders",
                recordId: returnOrderId.ToString(),
                operation: operation.ToString(),
                changedBy: changedBy,
                changeData: changeData,
                comment: comment
            );
        }


    }

}
