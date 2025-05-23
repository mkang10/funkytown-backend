using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class GetWareHouseStockByVariantHandler
    {
        private readonly IWareHousesStockRepository _warehouseStockRepository;

        public GetWareHouseStockByVariantHandler(IWareHousesStockRepository warehouseStockRepository)
        {
            _warehouseStockRepository = warehouseStockRepository;
        }

        /// <summary>
        /// Lấy tổng tồn kho của một variant trên toàn hệ thống.
        /// </summary>
        public async Task<int> HandleTotalStockAsync(int variantId)
        {
            return await _warehouseStockRepository.GetTotalStockByVariantAsync(variantId);
        }

        /// <summary>
        /// Lấy phân rã tồn kho của một variant theo từng cửa hàng.
        /// </summary>
        //public async Task<List<WarehouseStockResponse>> HandleStockBreakdownAsync(int variantId)
        //{
        //    var warehouseStocks = await _warehouseStockRepository.GetWareHouseStocksByVariantAsync(variantId);
        //    var result = warehouseStocks.Select(ss => new WarehouseStockResponse
        //    {
        //        WarehouseId = ss.WareHouseId,
        //        WarehouseName = ss..StoreName,
        //        StockQuantity = ss.StockQuantity
        //    }).ToList();

        //    return result;
        //}
    }
}
