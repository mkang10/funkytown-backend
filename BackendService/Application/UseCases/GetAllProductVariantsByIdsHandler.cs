using AutoMapper;
using Domain.DTO.Response;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class GetAllProductVariantsByIdsHandler
    {
        private readonly IProductRepository _productRepository;
        private readonly IRedisCacheService _cacheService;
        private readonly IMapper _mapper;

        public GetAllProductVariantsByIdsHandler(
            IProductRepository productRepository,
            IRedisCacheService cacheService,
            IMapper mapper)
        {
            _productRepository = productRepository;
            _cacheService = cacheService;
            _mapper = mapper;
        }

        public async Task<List<ProductVariantResponse>> Handle(List<int> variantIds)
        {
            if (variantIds == null || variantIds.Count == 0)
                return new List<ProductVariantResponse>();

            var productVariantResponses = new List<ProductVariantResponse>();
            var missingVariantIds = new List<int>();

            // 🟢 Kiểm tra cache trước
            foreach (var variantId in variantIds)
            {
                string instanceName = "ProductInstance";
                string cacheKey = $"{instanceName}:variant:{variantId}";
                var cachedVariant = await _cacheService.GetCacheAsync<ProductVariantResponse>(cacheKey);
                if (cachedVariant != null)
                {
                    productVariantResponses.Add(cachedVariant);
                }
                else
                {
                    missingVariantIds.Add(variantId);
                }
            }

            // 🔴 Nếu có ID chưa có trong cache, truy vấn database
            if (missingVariantIds.Count > 0)
            {
                var productVariants = await _productRepository.GetProductVariantsByIdsAsync(missingVariantIds);
                if (productVariants.Any())
                {
                    // Lấy tồn kho từ StoreStock
                    var stockQuantities = await _productRepository.GetProductVariantsStockAsync(missingVariantIds);

                    foreach (var productVariant in productVariants)
                    {
                        var variantResponse = _mapper.Map<ProductVariantResponse>(productVariant);
                        variantResponse.StockQuantity = stockQuantities.GetValueOrDefault(productVariant.VariantId, 0);

                        // ✅ Lưu vào cache
                        string cacheKey = $"variant:{productVariant.VariantId}";
                        await _cacheService.SetCacheAsync(cacheKey, variantResponse, TimeSpan.FromMinutes(30));

                        productVariantResponses.Add(variantResponse);
                    }
                }
            }

            return productVariantResponses;
        }
    }

}
