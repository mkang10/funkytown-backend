using Application.Enum;
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
    public class ProductRepository : IProductRepository
    {
        private readonly FtownContext _context;
        private readonly IRedisCacheService _cacheService;

        public async Task<PaginatedResponseDTO<ProductVariant>> GetAllAsync(int page, int pageSize, string? search = null)
        {
            // Xây dựng query và chỉ lọc status = "Draft"
            var query = _context.ProductVariants
                .Include(pv => pv.Product)
                    .ThenInclude(p => p.ProductImages)
                .Include(pv => pv.Size)
                .Include(pv => pv.Color)
                .AsQueryable();

            // Nếu có từ khóa tìm kiếm, lọc theo tên sản phẩm, màu sắc, kích thước
            if (!string.IsNullOrWhiteSpace(search))
            {
                string keyword = search.Trim().ToLower();
                query = query.Where(pv =>
                    pv.Product.Name.ToLower().Contains(keyword) ||
                    pv.Color.ColorName.ToLower().Contains(keyword) ||
                    pv.Size.SizeName.ToLower().Contains(keyword) ||
                    pv.Sku.ToLower().Contains(keyword)
                );
            }

            // Tổng số bản ghi sau khi lọc
            int totalRecords = await query.CountAsync();

            // Áp dụng phân trang
            var data = await query
                .OrderBy(pv => pv.VariantId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResponseDTO<ProductVariant>(data, totalRecords, page, pageSize);
        }
        public ProductRepository(FtownContext context, IRedisCacheService cacheService)
        {
            _context = context;
            _cacheService = cacheService;
        }
        public async Task<List<ProductVariant>> GetVariantsByFiltersAsync(
            string occasion,
            string style,
            int sizeId,
            CancellationToken ct = default)
        {
            return await _context.ProductVariants
                .Include(v => v.Product).ThenInclude(p => p.Category)
                .Include(v => v.Color)
                .Include(v => v.Size)
                .Where(v =>
                    v.SizeId == sizeId &&
                    v.Product.Occasion == occasion &&
                    v.Product.Style == style &&
                    v.Product.Status == "Active")
                .ToListAsync(ct);
        }
        public async Task<List<Product>> GetAllProductsWithVariantsAsync()
        {
            return await _context.Products
                .Include(p => p.ProductVariants)
                .Include(p => p.Category)
                .ToListAsync();
        }

        public async Task<Product?> GetProductByIdAsync(int productId)
        {
            return await _context.Products
                .Include(p => p.ProductVariants)
                    .ThenInclude(v => v.Size)
                .Include(p => p.ProductVariants)
                    .ThenInclude(v => v.Color)
                .Include(p => p.ProductVariants)
                    .ThenInclude(v => v.WareHousesStocks)
                .Include(p => p.Category)
                .Include(p => p.ProductImages) 
                .FirstOrDefaultAsync(p => p.ProductId == productId);
        }

		public async Task<bool> IsProductFavoriteAsync(int accountId, int productId)
		{
			return await _context.FavoriteProducts
				.AnyAsync(f => f.AccountId == accountId && f.ProductId == productId);
		}
		public async Task<ProductVariant?> GetProductVariantByIdAsync(int variantId)
        {
            return await _context.ProductVariants
                .Include(pv => pv.Product) 
                .Include(pv => pv.Size)
                .Include(pv => pv.Color)
                .FirstOrDefaultAsync(pv => pv.VariantId == variantId);
        }
        public async Task UpdateProductVariant(ProductVariant productVariant)
        {
            _context.ProductVariants.Update(productVariant);
            await _context.SaveChangesAsync();
        }
        public async Task<int> GetProductVariantStockAsync(int variantId)
        {
            return await _context.WareHousesStocks
                .Where(ss => ss.VariantId == variantId)
                .SumAsync(ss => ss.StockQuantity);
        }
        public async Task<List<Product>> GetPagedProductsWithVariantsAsync(int page, int pageSize)
        {
            return await _context.Products
                .AsNoTracking() // ✅ Thêm dòng này để bỏ tracking
                .Where(p => p.Status == ProductStatus.Online.ToString()
                         || p.Status == ProductStatus.Both.ToString())
                .Include(p => p.ProductVariants)
                    .ThenInclude(pv => pv.Color)
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
        public async Task<Product> CreateAsync(Product product)
        {
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                                .Include(p => p.ProductVariants)

                .ToListAsync();
        }

        public async Task<Product?> GetByIdAsync(int productId)
        {
            return await _context.Products
                .Include(p => p.ProductImages)
                .AsSplitQuery() // ✅ Thêm dòng này để tránh lỗi tracking duplicate entity
                .FirstOrDefaultAsync(p => p.ProductId == productId);
        }




        public async Task<Product?> GetByIdWithVariantsAsync(int productId)
            => await _context.Products.Include(p => p.ProductVariants)
            .Include(c => c.Category)
            .Include(p => p.ProductImages)
                                       .FirstOrDefaultAsync(p => p.ProductId == productId);

        public void Update(Product product) => _context.Products.Update(product);

        public Task<int> SaveChangesAsync() => _context.SaveChangesAsync();

        public void RemoveImage(ProductImage image)
        {
            _context.ProductImages.Remove(image);
        }

        public async Task<List<ProductVariant>> GetProductVariantsByIdsAsync(List<int> variantIds)
        {
            return await _context.ProductVariants
                .Include(pv => pv.Product) 
                .Include(pv => pv.Size)
                .Include(pv => pv.Color)
                .Where(pv => variantIds.Contains(pv.VariantId))
                .ToListAsync();
        }

        
        public async Task<Dictionary<int, int>> GetProductVariantsStockAsync(List<int> variantIds)
        {
            return await _context.WareHousesStocks
                .Where(ss => variantIds.Contains(ss.VariantId)) // Chỉ lấy các VariantId được yêu cầu
                .GroupBy(ss => ss.VariantId) // Gom nhóm theo VariantId
                .Select(g => new { VariantId = g.Key, StockQuantity = g.Sum(ss => ss.StockQuantity) }) // Tổng tồn kho
                .ToDictionaryAsync(x => x.VariantId, x => x.StockQuantity);
        }

        public async Task<ProductVariant?> GetProductVariantByDetailsAsync(int productId, string size, string color)
        {
            return await _context.ProductVariants
                .Include(pv => pv.Size)
                .Include(pv => pv.Color)
                .Where(pv => pv.ProductId == productId &&
                             pv.Size != null && pv.Size.SizeName.ToLower() == size.ToLower() &&
                             pv.Color != null && pv.Color.ColorCode.ToLower() == color.ToLower())
                .FirstOrDefaultAsync();
        }

        public async Task AddProductAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
        }
        public async Task AddProductsAsync(IEnumerable<Product> products)
        {
            _context.Products.AddRange(products);
            await _context.SaveChangesAsync();
        }

        public async Task AddProductVariantsAsync(List<ProductVariant> variants)
        {
            _context.ProductVariants.AddRange(variants);
            await _context.SaveChangesAsync();
        }

        public async Task AddProductImagesAsync(List<ProductImage> images)
        {
            _context.ProductImages.AddRange(images);
            await _context.SaveChangesAsync();
        }

		public async Task AddFavoriteAsync(int accountId, int productId)
		{
			bool exists = await _context.FavoriteProducts
				.AnyAsync(f => f.AccountId == accountId && f.ProductId == productId);

			if (!exists)
			{
				_context.FavoriteProducts.Add(new FavoriteProduct
				{
					AccountId = accountId,
					ProductId = productId,
					CreatedAt = DateTime.UtcNow
				});

				await _context.SaveChangesAsync();
			}
		}

		public async Task RemoveFavoriteAsync(int accountId, int productId)
		{
			var favorite = await _context.FavoriteProducts
				.FirstOrDefaultAsync(f => f.AccountId == accountId && f.ProductId == productId);

			if (favorite != null)
			{
				_context.FavoriteProducts.Remove(favorite);
				await _context.SaveChangesAsync();
			}
		}

		public async Task<List<Product>> GetFavoritePagedProductsAsync(int accountId, int page, int pageSize)
		{
			var query = from product in _context.Products
						join favorite in _context.FavoriteProducts
							on product.ProductId equals favorite.ProductId
						where favorite.AccountId == accountId
						   && (product.Status == ProductStatus.Online.ToString()
							   || product.Status == ProductStatus.Both.ToString())
						orderby favorite.CreatedAt descending // ✅ Sắp xếp mới nhất trước
						select product;

			return await query
				.Include(p => p.ProductVariants)
				.Include(p => p.Category)
				.Include(p => p.ProductImages)
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();
		}

        public async Task<List<Product>> GetProductsByCategoryNameAsync(string categoryName)
        {
            return await _context.Products
                .Where(p =>
                    (p.Status == ProductStatus.Online.ToString() ||
                     p.Status == ProductStatus.Both.ToString()) &&
                    p.Category.Name.ToLower() == categoryName.ToLower())
                .Include(p => p.Category)
                .Include(p => p.ProductVariants)
                .Include(p => p.ProductImages)
                .ToListAsync();
        }

        public async Task<List<Order>> GetCompletedOrdersWithDetailsAsync(DateTime? from, DateTime? to)
        {
            var query = _context.Orders
                .Where(o => o.Status == "completed" && o.CreatedDate.HasValue)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.ProductVariant)
                        .ThenInclude(pv => pv.Product)
                            .ThenInclude(p => p.Category)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.ProductVariant)
                        .ThenInclude(pv => pv.Product)
                            .ThenInclude(p => p.ProductImages)
                .Include(o => o.OrderDetails) // 🆕 Thêm dòng này để lấy màu
                    .ThenInclude(od => od.ProductVariant)
                        .ThenInclude(pv => pv.Color)
                .AsQueryable();

            if (from.HasValue)
                query = query.Where(o => o.CreatedDate.Value.Date >= from.Value.Date);

            if (to.HasValue)
                query = query.Where(o => o.CreatedDate.Value.Date <= to.Value.Date);

            return await query.ToListAsync();
        }
        public async Task<List<ProductVariant>> GetPublishedVariantsByProductIdAsync(int productId)
        {
            return await _context.ProductVariants
                .Where(v => v.ProductId == productId && v.Status == "Published")
                .Include(v => v.Size)   
                .Include(v => v.Color)  
                .Include(v => v.WareHousesStocks) 
                .ToListAsync();
        }
        public async Task<List<Product>> GetProductsByStyleNameAsync(string styleName, int page, int pageSize)
        {
            return await _context.Products
                .Where(p =>
                    (p.Status == ProductStatus.Online.ToString() || p.Status == ProductStatus.Both.ToString())
                    && p.Style == styleName)
                .Include(p => p.ProductVariants)
                    .ThenInclude(pv => pv.Color)
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

    }
}
