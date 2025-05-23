using Application.Interfaces;
using AutoMapper;
using Azure;
using Domain.DTO.Response;
using Domain.Entities;
using Domain.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class GetProductVariantByIdHandler
    {
        private readonly IProductRepository _productRepository;
        private readonly IRedisCacheService _cacheService;
        private readonly IMapper _mapper;
        private readonly IPromotionRepository _promotionRepository;
		private readonly IPromotionService _promotionService;
		public GetProductVariantByIdHandler(IProductRepository productRepository, IRedisCacheService cacheService, IMapper mapper, IPromotionRepository promotionRepository, IPromotionService promotionService)
        {
            _productRepository = productRepository;
            _cacheService = cacheService;
            _mapper = mapper;
            _promotionRepository = promotionRepository;
            _promotionService = promotionService;
        }

        public async Task<ProductVariantResponse?> Handle(int variantId)
        {
            string instanceName = "ProductInstance";
            string cacheKey = $"{instanceName}:variant:{variantId}";

            // 🔍 Kiểm tra cache trước khi gọi database
            var cachedVariant = await _cacheService.GetCacheAsync<ProductVariantResponse>(cacheKey);
            if (cachedVariant != null)
                return cachedVariant;

            // ❌ Không có cache, truy vấn database
            var productVariant = await _productRepository.GetProductVariantByIdAsync(variantId);
            if (productVariant == null)
                return null;

            int stockQuantity = await _productRepository.GetProductVariantStockAsync(variantId);
            var variantResponse = _mapper.Map<ProductVariantResponse>(productVariant);
            variantResponse.StockQuantity = stockQuantity;

            // 🔹 Lấy danh sách khuyến mãi đang hoạt động
            var promotions = await _promotionRepository.GetActiveProductPromotionsAsync();

			_promotionService.ApplyPromotion(
			productVariant.ProductId,
			variantResponse.Price,
			promotions,
			out var discountedPrice,
			out var promotionTitle);

			variantResponse.DiscountedPrice = discountedPrice;
			variantResponse.PromotionTitle = promotionTitle;

			// ✅ Lưu vào cache với TTL 30 phút
			await _cacheService.SetCacheAsync(cacheKey, variantResponse, TimeSpan.FromMinutes(5));

            return variantResponse;
        }

    }

}
