using Domain.DTO.Response;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class WareHousesStockRepository : IWareHousesStockRepository
    {
        private readonly FtownContext _context;
        private readonly IProductVarRepos _varRepos;
        private readonly IStaffDetailRepository _staffDetail;



        public WareHousesStockRepository(FtownContext context, IProductVarRepos varRepos, IStaffDetailRepository staffDetail)
        {
            _context = context;
            _varRepos = varRepos;
            _staffDetail = staffDetail;

        }

        public async Task UpdateWarehouseStockAsync(
    Import import,
    int staffId,
    List<int> confirmedStoreDetailIds)
        {
            var accountId = await _staffDetail.GetAccountIdByStaffIdAsync(staffId);
            // Chỉ lấy những storeDetail vừa xác nhận
            var detailStorePairs = import.ImportDetails
        .SelectMany(d => d.ImportStoreDetails
            .Where(sd =>
                sd.ActualReceivedQuantity.HasValue &&
                sd.WarehouseId.HasValue &&
                confirmedStoreDetailIds.Contains(sd.ImportStoreId)
            )
            .Select(sd => new { Detail = d, Store = sd }))
        .ToList();

            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var pair in detailStorePairs)
                {
                    var detail = pair.Detail;
                    var store = pair.Store;

                    int qty = store.ActualReceivedQuantity!.Value;
                    int whId = store.WarehouseId!.Value;
                    int variantId = detail.ProductVariantId;
                    string productName = detail.ProductVariant?.Product?.Name?.Trim() ?? "[Unknown]";

                    // Skip if already audited for same change
                    bool alreadyAudited = await _context.WareHouseStockAudits.AnyAsync(a =>
                        a.WareHouseStock.WareHouseId == whId &&
                        a.WareHouseStock.VariantId == variantId &&
                        a.QuantityChange == qty &&
                        a.Note.Contains(productName)
                    );
                    if (alreadyAudited)
                        continue;

                    // Find or create stock record
                    var stock = await _context.WareHousesStocks
                        .FirstOrDefaultAsync(s => s.WareHouseId == whId && s.VariantId == variantId);

                    string action;
                    if (stock == null)
                    {
                        stock = new WareHousesStock
                        {
                            WareHouseId = whId,
                            VariantId = variantId,
                            StockQuantity = qty
                        };
                        _context.WareHousesStocks.Add(stock);
                        action = "CREATE";
                    }
                    else
                    {
                        stock.StockQuantity += qty;
                        action = "INCREASE";
                    }

                    // Add audit log
                    _context.WareHouseStockAudits.Add(new WareHouseStockAudit
                    {
                        WareHouseStock = stock,
                        Action = action,
                        QuantityChange = qty,
                        ActionDate = DateTime.Now,
                        ChangedBy = accountId,
                        Note = $"Nhập kho thành công !"
                    });

                    // Mark store detail as processed
                    store.Status = "Success";
                    _context.ImportStoreDetails.Update(store);
                }

                // Save all changes and commit
                await _context.SaveChangesAsync();
                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task UpdateDispatchWarehouseStockAsync(
    Dispatch dispatch,
    int staffId,
    List<int> confirmedStoreDetailIds)
        {
            // 1. Bắt đầu transaction
            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                // 2. Lấy accountId từ staffId
                var accountId = await _staffDetail.GetAccountIdByStaffIdAsync(staffId);


                // 3. Duyệt qua các DispatchDetail và chỉ xử lý những storeDetail vừa confirmed
                foreach (var dispatchDetail in dispatch.DispatchDetails)
                {
                    int variantId = dispatchDetail.VariantId;

                    foreach (var storeDetail in dispatchDetail.StoreExportStoreDetails
                                 .Where(sd => confirmedStoreDetailIds.Contains(sd.DispatchStoreDetailId)))
                    {
                        // 3.1. Tính số lượng thực đã dispatch
                        int actualDispatchedQty = storeDetail.ActualQuantity ?? storeDetail.AllocatedQuantity;
                        int warehouseId = storeDetail.WarehouseId;

                        // 3.2. Lấy hoặc tạo record kho
                        var wareHouseStock = await _context.WareHousesStocks
                            .FirstOrDefaultAsync(ws => ws.WareHouseId == warehouseId && ws.VariantId == variantId);

                        string action;
                        if (wareHouseStock == null)
                        {
                            wareHouseStock = new WareHousesStock
                            {
                                WareHouseId = warehouseId,
                                VariantId = variantId,
                                StockQuantity = -actualDispatchedQty
                            };
                            _context.WareHousesStocks.Add(wareHouseStock);
                            action = "CREATE";
                        }
                        else
                        {
                            wareHouseStock.StockQuantity -= actualDispatchedQty;
                            action = "DECREASE";
                        }

                        // 3.3. Tạo audit log, EF Core sẽ tự liên kết wareHouseStock
                        var stockAudit = new WareHouseStockAudit
                        {
                            WareHouseStock = wareHouseStock,
                            Action = action,
                            QuantityChange = -actualDispatchedQty,
                            ActionDate = DateTime.Now,
                            ChangedBy = accountId,
                            Note = "Đơn hàng đã được xuất !"
                        };
                        _context.WareHouseStockAudits.Add(stockAudit);
                    }
                }

                // 4. Lưu chung một lần và commit
                await _context.SaveChangesAsync();
                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }





        public async Task<WareHousesStock?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.WareHousesStocks
                // Include Variant và các con của nó
                .Include(ws => ws.Variant)
                    .ThenInclude(v => v.Product)
                .Include(ws => ws.Variant)
                    .ThenInclude(v => v.Color)
                .Include(ws => ws.Variant)
                    .ThenInclude(v => v.Size)
                // Include thông tin kho
                .Include(ws => ws.WareHouse)
                // Include audit history và user tạo thay đổi
                .Include(ws => ws.WareHouseStockAudits)
                    .ThenInclude(a => a.ChangedByNavigation)
                // Nếu bạn không sửa đổi entity thì tốt nhất dùng AsNoTracking()
                .AsNoTracking()
                .FirstOrDefaultAsync(ws => ws.WareHouseStockId == id);
        }


        public async Task<IEnumerable<WareHousesStock>> GetByWarehouseIdAsync(int warehouseId)
        {
            return await _context.WareHousesStocks
                .Where(ws => ws.WareHouseId == warehouseId)
                .Include(ws => ws.Variant).ThenInclude(v => v.Product)
                .Include(ws => ws.Variant).ThenInclude(v => v.Size)
                .Include(ws => ws.Variant).ThenInclude(v => v.Color)
                .Include(ws => ws.WareHouse)
                .Include(ws => ws.WareHouseStockAudits)
                .ToListAsync();
        }

        public async Task<bool> HasStockAsync(int productId, int sizeId, int colorId)
        {
            var variantId = await _varRepos.GetVariantIdAsync(productId, sizeId, colorId);
            if (variantId == null)
                return false;

            return await _context.WareHousesStocks
                .AnyAsync(ws => ws.VariantId == variantId && ws.StockQuantity > 0);
        }


        public async Task<IEnumerable<WareHousesStock>> GetByWarehouseAsync(int warehouseId)
            => await _context.WareHousesStocks
                .Where(s => s.WareHouseId == warehouseId)
                .ToListAsync();

        //public async Task<WareHousesStock?> GetByWarehouseAndVariantAsync(int warehouseId, int variantId)
        //    => await _context.WareHousesStocks
        //        .FirstOrDefaultAsync(s => s.WareHouseId == warehouseId && s.VariantId == variantId);

        public async Task<IEnumerable<WareHousesStock>> GetAllByVariantAsync(int variantId)
            => await _context.WareHousesStocks
                .Where(s => s.VariantId == variantId)
                .ToListAsync();

        public Task UpdateAsync(WareHousesStock stock)
        {
            _context.WareHousesStocks.Update(stock);
            return Task.CompletedTask;
        }
        public async Task UpdateWarehouseStockAsync(Import import, int staffId)
        {
            // Chỉ xử lý các import có ImportType là Purchase (dùng Trim để bỏ khoảng trắng thừa)
            if (import.ImportType?.Trim() != "Purchase")
            {
                // Nếu không phải Purchase, không thực hiện cập nhật tồn kho
                return;
            }
            // Duyệt qua từng ImportDetail trong Import
            foreach (var importDetail in import.ImportDetails)
            {
                int variantId = importDetail.ProductVariantId;
                foreach (var storeDetail in importDetail.ImportStoreDetails)
                {
                    if (!storeDetail.ActualReceivedQuantity.HasValue || !storeDetail.WarehouseId.HasValue)
                    {
                        // Xử lý trường hợp giá trị null, ví dụ log lỗi, gán giá trị mặc định, hoặc bỏ qua cập nhật.
                        continue; // hoặc throw exception với thông báo rõ ràng
                    }
                    int actualReceivedQuan = storeDetail.ActualReceivedQuantity.Value;
                    int warehouseId = storeDetail.WarehouseId.Value;

                    // Tìm WareHousesStock theo WarehouseId và VariantId
                    var wareHouseStock = await _context.WareHousesStocks
                        .FirstOrDefaultAsync(ws => ws.WareHouseId == warehouseId && ws.VariantId == variantId);

                    string auditAction = "";
                    if (wareHouseStock == null)
                    {
                        // Chưa có: tạo bản ghi mới
                        wareHouseStock = new WareHousesStock
                        {
                            WareHouseId = warehouseId,
                            VariantId = variantId,
                            StockQuantity = actualReceivedQuan
                        };
                        _context.WareHousesStocks.Add(wareHouseStock);
                        // Lưu ngay để nhận WareHouseStockId
                        await _context.SaveChangesAsync();
                        auditAction = "CREATE";
                    }
                    else
                    {
                        // Đã có: tăng số lượng tồn kho
                        wareHouseStock.StockQuantity += actualReceivedQuan;
                        auditAction = "INCREASE";
                    }

                    // Tạo WareHouseStockAudit cho giao dịch cập nhật tồn kho
                    var stockAudit = new WareHouseStockAudit
                    {
                        WareHouseStockId = wareHouseStock.WareHouseStockId, // Đã được gán sau SaveChanges nếu là bản ghi mới
                        Action = auditAction,
                        QuantityChange = actualReceivedQuan,
                        ActionDate = DateTime.Now,
                        ChangedBy = staffId,
                        Note = $"Updated via Import Done. ImportDetailId: {importDetail.ImportDetailId}, ImportStoreId: {storeDetail.ImportStoreId}"
                    };
                    _context.WareHouseStockAudits.Add(stockAudit);
                }
            }

            // Cuối cùng lưu lại các bản ghi audit (và các cập nhật tồn kho nếu có bản ghi cũ)
            await _context.SaveChangesAsync();
        }



        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task UpdateWarehouseStockForSingleDetailAsync(ImportStoreDetail storeDetail, int productVariantId, int staffId)
        {
            // Lấy số lượng thực tế nhận được và warehouseId từ storeDetail
            int actualReceivedQuan = (int)storeDetail.ActualReceivedQuantity;
            int warehouseId = (int)storeDetail.WarehouseId;

            // Tìm WareHousesStock theo WarehouseId và VariantId
            var wareHouseStock = await _context.WareHousesStocks
                .FirstOrDefaultAsync(ws => ws.WareHouseId == warehouseId && ws.VariantId == productVariantId);

            string auditAction = "";
            if (wareHouseStock == null)
            {
                // Nếu chưa có bản ghi kho: tạo mới
                wareHouseStock = new WareHousesStock
                {
                    WareHouseId = warehouseId,
                    VariantId = productVariantId,
                    StockQuantity = actualReceivedQuan
                };
                _context.WareHousesStocks.Add(wareHouseStock);
                // Lưu ngay để nhận WareHouseStockId
                await _context.SaveChangesAsync();
                auditAction = "CREATE";
            }
            else
            {
                // Nếu đã có: tăng số lượng tồn kho
                wareHouseStock.StockQuantity += actualReceivedQuan;
                auditAction = "INCREASE";
            }

            // Tạo WareHouseStockAudit cho giao dịch cập nhật kho
            var stockAudit = new WareHouseStockAudit
            {
                WareHouseStockId = wareHouseStock.WareHouseStockId, // Đã có sau SaveChanges nếu là bản ghi mới
                Action = auditAction,
                QuantityChange = actualReceivedQuan,
                ActionDate = DateTime.Now,
                ChangedBy = staffId,
                Note = $"Updated via Import Done. ImportStoreId: {storeDetail.ImportStoreId}"
            };
            _context.WareHouseStockAudits.Add(stockAudit);

            // Lưu lại các thay đổi cho WareHouseStock và WareHouseStockAudit
            await _context.SaveChangesAsync();
        }


        public async Task UpdateDispatchWarehouseStockAsync(Dispatch dispatch, int staffId)
        {


            // Duyệt qua từng DispatchDetail trong Dispatch
            foreach (var dispatchDetail in dispatch.DispatchDetails)
            {
                int variantId = dispatchDetail.VariantId;
                foreach (var storeDetail in dispatchDetail.StoreExportStoreDetails)
                {
                    // Giả sử: Nếu không có giá trị ActualDispatchedQuantity, ta dùng AllocatedQuantity
                    int actualDispatchedQuan = storeDetail.ActualQuantity.HasValue
                        ? storeDetail.ActualQuantity.Value
                        : storeDetail.AllocatedQuantity;

                    int warehouseId = storeDetail.WarehouseId; // WarehouseId là int, không cần nullable

                    // Tìm WareHousesStock theo WarehouseId và VariantId
                    var wareHouseStock = await _context.WareHousesStocks
                        .FirstOrDefaultAsync(ws => ws.WareHouseId == warehouseId && ws.VariantId == variantId);

                    string auditAction = "";
                    if (wareHouseStock == null)
                    {
                        // Nếu không có bản ghi: tạo mới với số lượng âm (giảm kho)
                        wareHouseStock = new WareHousesStock
                        {
                            WareHouseId = warehouseId,
                            VariantId = variantId,
                            StockQuantity = -actualDispatchedQuan
                        };
                        _context.WareHousesStocks.Add(wareHouseStock);
                        // Lưu ngay để nhận WareHouseStockId
                        await _context.SaveChangesAsync();
                        auditAction = "CREATE";
                    }
                    else
                    {
                        // Nếu đã có: giảm số lượng tồn kho
                        wareHouseStock.StockQuantity -= actualDispatchedQuan;
                        auditAction = "DECREASE";
                    }

                    // Tạo WareHouseStockAudit cho giao dịch cập nhật tồn kho
                    var stockAudit = new WareHouseStockAudit
                    {
                        WareHouseStockId = wareHouseStock.WareHouseStockId, // Đã được gán sau SaveChanges nếu là bản ghi mới
                        Action = auditAction,
                        // Ghi nhận sự thay đổi dưới dạng giá trị âm để thể hiện việc giảm kho
                        QuantityChange = -actualDispatchedQuan,
                        ActionDate = DateTime.Now,
                        ChangedBy = staffId,
                        Note = $"Updated via Dispatch Done. DispatchDetailId: {dispatchDetail.DispatchDetailId}, DispatchStoreDetailId: {storeDetail.DispatchStoreDetailId}"
                    };
                    _context.WareHouseStockAudits.Add(stockAudit);
                }
            }

            // Cuối cùng lưu lại các bản ghi audit và cập nhật kho
            await _context.SaveChangesAsync();
        }


        public async Task UpdateWarehouseStockForSingleDispatchDetailAsync(StoreExportStoreDetail storeDetail, int productVariantId, int staffId)
        {
            // Lấy số lượng thực tế đã dispatch (nếu có, nếu không lấy AllocatedQuantity)
            int actualDispatchedQuan = storeDetail.ActualQuantity.HasValue
                ? storeDetail.ActualQuantity.Value
                : storeDetail.AllocatedQuantity;

            int warehouseId = storeDetail.WarehouseId; // Giả sử WarehouseId là int (không nullable)

            // Tìm WareHousesStock theo WarehouseId và VariantId
            var wareHouseStock = await _context.WareHousesStocks
                .FirstOrDefaultAsync(ws => ws.WareHouseId == warehouseId && ws.VariantId == productVariantId);

            string auditAction = "";
            if (wareHouseStock == null)
            {
                // Nếu chưa có bản ghi kho, tạo mới với StockQuantity âm để biểu thị việc giảm số lượng
                wareHouseStock = new WareHousesStock
                {
                    WareHouseId = warehouseId,
                    VariantId = productVariantId,
                    StockQuantity = -actualDispatchedQuan
                };
                _context.WareHousesStocks.Add(wareHouseStock);
                // Lưu ngay để nhận WareHouseStockId
                await _context.SaveChangesAsync();
                auditAction = "CREATE";
            }
            else
            {
                // Nếu đã có: giảm số lượng tồn kho
                wareHouseStock.StockQuantity -= actualDispatchedQuan;
                auditAction = "DECREASE";
            }

            // Tạo WareHouseStockAudit cho giao dịch cập nhật kho (ghi nhận số lượng thay đổi dưới dạng số âm)
            var stockAudit = new WareHouseStockAudit
            {
                WareHouseStockId = wareHouseStock.WareHouseStockId, // Đã được gán sau SaveChanges nếu là bản ghi mới
                Action = auditAction,
                QuantityChange = -actualDispatchedQuan, // Số lượng giảm nên giá trị âm
                ActionDate = DateTime.Now,
                ChangedBy = staffId,
                Note = $"Updated via Dispatch Done. DispatchStoreDetailId: {storeDetail.DispatchStoreDetailId}"
            };
            _context.WareHouseStockAudits.Add(stockAudit);

            // Lưu lại các thay đổi cho WareHousesStock và WareHouseStockAudit
            await _context.SaveChangesAsync();
        }
        public async Task<PaginatedResponseDTO<WarehouseStockDto>> GetAllWareHouse(int page, int pageSize, CancellationToken cancellationToken = default)
        {
            var query = _context.WareHousesStocks
                .AsNoTracking()
                .Select(ws => new WarehouseStockDto
                {
                    WareHouseStockId = ws.WareHouseStockId,
                    VariantId = ws.VariantId,
                    StockQuantity = ws.StockQuantity,
                    WareHouseId = ws.WareHouseId,
                    WarehouseName = ws.WareHouse.WarehouseName,
                    FullProductName = ws.Variant.Product.Name
                                        + " " + ws.Variant.Color.ColorName
                                        + " " + ws.Variant.Size.SizeName
                });

            var total = await query.CountAsync(cancellationToken);
            var data = await query
                            .Skip((page - 1) * pageSize)
                            .Take(pageSize)
                            .ToListAsync(cancellationToken);

            return new PaginatedResponseDTO<WarehouseStockDto>(data, total, page, pageSize);
        }

        public async Task<int> GetStockQuantityAsync(int warehouseId, int variantId)
        {
            var warehouseStock = await _context.WareHousesStocks
                .FirstOrDefaultAsync(ss => ss.WareHouseId == warehouseId && ss.VariantId == variantId);
            return warehouseStock?.StockQuantity ?? 0;
        }

        public async Task<int> GetTotalStockByVariantAsync(int variantId)
        {
            int total = await _context.WareHousesStocks
                .Where(ss => ss.VariantId == variantId)
                .SumAsync(ss => (int?)ss.StockQuantity) ?? 0;
            return total;
        }

        public async Task<List<WareHousesStock>> GetWareHouseStocksByVariantAsync(int variantId)
        {
            return await _context.WareHousesStocks
                .Include(ss => ss.WareHouse)
                .Where(ss => ss.VariantId == variantId)
                .ToListAsync();
        }
        public async Task<bool> UpdateStockAfterOrderAsync(int warehouseId, List<(int VariantId, int Quantity)> stockUpdates)
        {
            // Lấy danh sách VariantId cần trừ tồn kho
            var variantIds = stockUpdates.Select(s => s.VariantId).ToList();

            // Lấy tồn kho tại kho duy nhất
            var warehouseStocks = await _context.WareHousesStocks
                .Where(s => s.WareHouseId == warehouseId && variantIds.Contains(s.VariantId))
                .ToListAsync();

            // Kiểm tra tất cả mặt hàng có đủ tồn kho không
            foreach (var update in stockUpdates)
            {
                var stockItem = warehouseStocks.FirstOrDefault(s => s.VariantId == update.VariantId);

                if (stockItem == null || stockItem.StockQuantity < update.Quantity)
                {
                    return false; // Không đủ hàng, huỷ luôn
                }
            }

            // Nếu tất cả đều đủ hàng thì tiến hành trừ tồn kho
            foreach (var update in stockUpdates)
            {
                var stockItem = warehouseStocks.First(s => s.VariantId == update.VariantId);
                stockItem.StockQuantity -= update.Quantity;
            }

            _context.WareHousesStocks.UpdateRange(warehouseStocks);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RestoreStockAfterCancelAsync(int warehouseId, List<(int VariantId, int Quantity)> stockUpdates)
        {
            var variantIds = stockUpdates.Select(s => s.VariantId).ToList();

            var warehouseStocks = await _context.WareHousesStocks
                .Where(s => s.WareHouseId == warehouseId && variantIds.Contains(s.VariantId))
                .ToListAsync();

            foreach (var update in stockUpdates)
            {
                var stockItem = warehouseStocks.FirstOrDefault(s => s.VariantId == update.VariantId);
                if (stockItem == null)
                {
                    // Nếu chưa có thì tạo mới luôn
                    stockItem = new WareHousesStock
                    {
                        WareHouseId = warehouseId,
                        VariantId = update.VariantId,
                        StockQuantity = 0
                    };
                    _context.WareHousesStocks.Add(stockItem);
                }

                stockItem.StockQuantity += update.Quantity;
            }

            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<WareHousesStock> GetByWarehouseAndVariantAsync(int warehouseId, int variantId)
        {
            return await _context.WareHousesStocks
                            .AsNoTracking()
                            .FirstOrDefaultAsync(s => s.WareHouseId == warehouseId
                                                   && s.VariantId == variantId);
        }
    }
}
