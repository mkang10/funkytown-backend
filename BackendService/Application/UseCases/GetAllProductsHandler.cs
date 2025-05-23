using Application.Interfaces;
using AutoMapper;
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
    public class GetAllProductsHandler
    {
        private readonly IProductRepository _productRepository;
        private readonly IRedisCacheService _cacheService;
        private readonly IMapper _mapper;
        private readonly IPromotionRepository _promotionRepository;
		private readonly IPromotionService _promotionService;
		public GetAllProductsHandler(IProductRepository productRepository, IMapper mapper, IRedisCacheService cacheService, IPromotionRepository promotionRepository, IPromotionService promotionService)
        {
            _productRepository = productRepository;
            _mapper = mapper;
            _cacheService = cacheService;
            _promotionRepository = promotionRepository;
            _promotionService = promotionService;
        }

        public async Task<List<ProductListResponse>> Handle(int page, int pageSize)
        {
            string instanceName = "ProductInstance"; // 🔹 Định nghĩa instance name (có thể lấy từ config)
            string cacheKey = $"{instanceName}:products:view-all:page:{page}:size:{pageSize}";

            // 🔍 Kiểm tra cache trước khi gọi database
            var cachedProducts = await _cacheService.GetCacheAsync<List<ProductListResponse>>(cacheKey);
            if (cachedProducts != null)
                return cachedProducts;

            // ❌ Nếu cache không có, gọi Repository để lấy dữ liệu từ database
            var products = await _productRepository.GetPagedProductsWithVariantsAsync(page, pageSize);

            if (products == null || !products.Any())
                return new List<ProductListResponse>();

            // 🔹 Lấy danh sách khuyến mãi đang hoạt động
            var promotions = await _promotionRepository.GetActiveProductPromotionsAsync();

            var productList = products.Select(p =>
            {
                var dto = _mapper.Map<ProductListResponse>(p); // dto.Price đã được map từ ProductVariants

                _promotionService.ApplyPromotion(
                    dto.ProductId,
                    dto.Price, // ✅ dùng dto.Price thay vì p.Price
                    promotions,
                    out var discounted,
                    out var promoTitle);

                dto.DiscountedPrice = discounted;
                dto.PromotionTitle = promoTitle;

                return dto;
            }).ToList();
            // ✅ Lưu vào cache với TTL 10 phút
            await _cacheService.SetCacheAsync(cacheKey, productList, TimeSpan.FromMinutes(10));

            return productList;
        }

    }

}
