using Domain.DTO.Request;
using Domain.DTO.Response;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class UpdateStockAfterOrderHandler
    {
        private readonly IWareHousesStockRepository _wareHousesStockRepository;
        public UpdateStockAfterOrderHandler(IWareHousesStockRepository wareHousesStockRepository)
        {
            _wareHousesStockRepository = wareHousesStockRepository;
        }
        public async Task<StockUpdateResponse> HandleAsync(StockUpdateRequest request)
        {
            // Chuyển đổi danh sách StockItemResponse thành danh sách tuple (VariantId, Quantity)
            var stockUpdates = request.Items
                                      .Select(i => (VariantId: i.VariantId, Quantity: i.Quantity))
                                      .ToList();

            // Gọi repository để cập nhật tồn kho trong DB
            bool success = await _wareHousesStockRepository.UpdateStockAfterOrderAsync(request.WarehouseId, stockUpdates);

            // Trả về response
            return new StockUpdateResponse
            {
                Success = success,
                Message = success ? "Cập nhật tồn kho thành công." : "Cập nhật tồn kho thất bại.",
            };
        }

        public async Task<StockUpdateResponse> HandleRestoreStockAsync(StockUpdateRequest request)
        {
            var stockRestores = request.Items
                                       .Select(i => (VariantId: i.VariantId, Quantity: i.Quantity))
                                       .ToList();

            bool success = await _wareHousesStockRepository.RestoreStockAfterCancelAsync(
                request.WarehouseId, stockRestores
            );

            return new StockUpdateResponse
            {
                Success = success,
                Message = success ? "Khôi phục tồn kho thành công." : "Khôi phục tồn kho thất bại.",
            };
        }
    }
}

