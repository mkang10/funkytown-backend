
using Application.Interfaces;
using AutoMapper;
using Domain.DTO.Request;
using Domain.DTO.Response;
using Domain.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class GetProductVariantByDetailsHandler
    {
        private readonly IProductRepository _productRepository;
        private readonly IRedisCacheService _cacheService;
        private readonly IMapper _mapper;
        private readonly IWareHousesStockRepository _wareHousesStockRepository;
        private readonly IPromotionRepository _promotionRepository;
		private readonly IPromotionService _promotionService;
		public GetProductVariantByDetailsHandler(IProductRepository productRepository,
                                                 IRedisCacheService cacheService,
                                                 IMapper mapper,
                                                 IWareHousesStockRepository wareHousesStockRepository,
                                                 IPromotionRepository promotionRepository,
                                                 IPromotionService promotionService)
        {
            _productRepository = productRepository;
            _cacheService = cacheService;
            _mapper = mapper;
            _wareHousesStockRepository = wareHousesStockRepository;
            _promotionRepository = promotionRepository;
            _promotionService = promotionService;
        }

        public async Task<ProductVariantResponse?> HandleAsync(GetProductVariantByDetailsRequest request)
        {
            string instanceName = "ProductInstance"; // 🔹 Lấy từ config nếu cần
            string cacheKey = $"{instanceName}:product:{request.ProductId}"; // 🔹 Trùng với cacheKey của ProductDetail

            // Kiểm tra cache trong Redis trước
            var cachedProduct = await _cacheService.GetCacheAsync<ProductDetailResponseInven>(cacheKey);
            if (cachedProduct != null && cachedProduct.Variants != null)
            {
                // Tìm nhanh biến thể cần lấy trong danh sách đã cache
                var variant = cachedProduct.Variants.FirstOrDefault(v =>
                    v.Size.Trim().Equals(request.Size.Trim(), StringComparison.OrdinalIgnoreCase) &&
                    v.Color.Trim().Equals(request.Color.Trim(), StringComparison.OrdinalIgnoreCase));

                if (variant != null)
                    return variant;
            }

            // Nếu cache không có, truy vấn DB như bình thường
            var productVariant = await _productRepository.GetProductVariantByDetailsAsync(request.ProductId, request.Size, request.Color);
            if (productVariant == null)
                return null;

            int stockQuantity = await _wareHousesStockRepository.GetStockQuantityAsync(2, productVariant.VariantId);

            // Lấy danh sách khuyến mãi áp dụng cho sản phẩm
            var promotions = await _promotionRepository.GetActiveProductPromotionsAsync();

            var variantResponse = _mapper.Map<ProductVariantResponse>(productVariant);
            variantResponse.StockQuantity = stockQuantity;

			// Áp dụng khuyến mãi qua service
			_promotionService.ApplyPromotion(
				request.ProductId,
				variantResponse.Price,
				promotions,
				out var discountedPrice,
				out var promotionTitle);

			variantResponse.DiscountedPrice = discountedPrice;
			variantResponse.PromotionTitle = promotionTitle;

			return variantResponse;
		}

    }

}
