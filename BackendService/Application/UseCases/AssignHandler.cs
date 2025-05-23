using AutoMapper;
using Domain.DTO.Request;
using Domain.DTO.Response;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Domain.DTO.Response.OrderAssigmentRes;

namespace Application.UseCases
{
    public class AssignStaffHandler
    {
        private readonly IImportRepos _invenRepos;
        private readonly IStaffDetailRepository _staffDetailRepos;
        private readonly IDispatchRepos _dispatchRepos;
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;

        public AssignStaffHandler(IMapper mapper, IOrderRepository orderRepository, IImportRepos invenRepos,
                                      IStaffDetailRepository staffDetailRepos, IDispatchRepos dispatchRepos)
        {
            _invenRepos = invenRepos;
            _staffDetailRepos = staffDetailRepos;
            _dispatchRepos = dispatchRepos;
            _orderRepository = orderRepository;
            _mapper = mapper;
        }


        public async Task<ResponseDTO<bool>> AssignStaffAccountAsync(int importId, int staffId)
        {
            // 1. Load import kèm ImportDetails → ImportStoreDetails
            var inventoryImport = await _invenRepos.GetByIdAssignAsync(importId);
            if (inventoryImport == null)
                return new ResponseDTO<bool>(false, false, "Inventory import not found");

            // 2. Load staffDetail (trong đó có StoreId)
            var staffDetail = await _staffDetailRepos.GetByIdAsync(staffId);
            if (staffDetail == null)
                return new ResponseDTO<bool>(false, false, "Staff detail not found");

            // 3. Lọc ra chỉ những ImportStoreDetail có ImportStoreId giống StoreId của nhân viên
            var targetStoreDetails = inventoryImport
                .ImportDetails
                .SelectMany(d => d.ImportStoreDetails)
                .ToList();

            if (!targetStoreDetails.Any())
                return new ResponseDTO<bool>(false, false, "Không có store detail phù hợp để gán");

            // 4. Gán staff và cập nhật status cho đúng những store details đó
            foreach (var sd in targetStoreDetails)
            {
                sd.StaffDetailId = staffDetail.StaffDetailId;
                sd.Status = "Processing";
            }

            // 5. (Tùy chọn) Cập nhật status chung của import
            inventoryImport.Status = "Processing";

            // 6. Lưu thay đổi vào DB
            await _invenRepos.UpdateAsync(inventoryImport);

            return new ResponseDTO<bool>(true, true, "Staff đã được gán cho đúng store detail");
        }


        public async Task<ResponseDTO<bool>> AssignStaffDispatchAccountAsync(int dispatchId, int staffId)
        {
            // 1. Load dispatch kèm các DispatchDetails -> StoreExportStoreDetails
            var dispatch = await _dispatchRepos.GetByIdDispatchAssignAsync(dispatchId);
            if (dispatch == null)
                return new ResponseDTO<bool>(false, false, "Dispatch not found");

            // 2. Load staffDetail (có StoreId)
            var staffDetail = await _staffDetailRepos.GetByIdAsync(staffId);
            if (staffDetail == null)
                return new ResponseDTO<bool>(false, false, "Staff detail not found for the given staff id");

            // 3. Lọc ra chỉ những export-detail có ExportStoreId == staffDetail.StoreId
            var targetStoreDetails = dispatch
                .DispatchDetails
                .SelectMany(d => d.StoreExportStoreDetails)
                .ToList();

            if (!targetStoreDetails.Any())
                return new ResponseDTO<bool>(false, false, "No matching store export details to assign");

            // 4. Gán staff và cập nhật status cho đúng những detail đó
            foreach (var sd in targetStoreDetails)
            {
                sd.StaffDetailId = staffDetail.StaffDetailId;
                sd.Status = "Processing";
            }

            // 5. (Tùy chọn) cập nhật trạng thái chung của dispatch
            dispatch.Status = "Processing";

            // 6. Lưu vào DB
            await _dispatchRepos.UpdateAsync(dispatch);

            return new ResponseDTO<bool>(true, true, "Staff assigned to matching export store details successfully");
        }

        public async Task<ResponseDTO<OrderAssignmentResponseDTO>> AssignStaffAsync(AssignStaffDTO dto)
        {
            var assignment = await _orderRepository.GetByOrderIdAsync(dto.OrderId);
            if (assignment == null)
            {
                return new ResponseDTO<OrderAssignmentResponseDTO>(
                    null!,
                    false,
                    $"Không tìm thấy phân công nào cho OrderId = {dto.OrderId}"
                );
            }

            _mapper.Map(dto, assignment);
            assignment.Order.Status = "Processing";

            await _orderRepository.SaveChangesAsync();

            var responseDto = _mapper.Map<OrderAssignmentResponseDTO>(assignment);
            return new ResponseDTO<OrderAssignmentResponseDTO>(
                responseDto,
                true,
                "Gán nhân viên thành công."
            );
        }



    }
}
