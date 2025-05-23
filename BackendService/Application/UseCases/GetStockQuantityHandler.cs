using Domain.DTO.Response;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class GetStockQuantityHandler
    {
        private readonly IWareHousesStockRepository _warehouseStockRepository;

        public GetStockQuantityHandler(IWareHousesStockRepository warehouseStockRepository)
        {
            _warehouseStockRepository = warehouseStockRepository;
        }

        public async Task<StockQuantityResponse> HandleAsync(int warehouseId, int productVariantId)
        {
            int stockQuantity = await _warehouseStockRepository.GetStockQuantityAsync(warehouseId, productVariantId);

            return new StockQuantityResponse
            {
                WarehouseId = warehouseId,
                ProductVariantId = productVariantId,
                StockQuantity = stockQuantity
            };
        }
    }
}
