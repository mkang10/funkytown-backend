using Application.Interfaces;
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
    public class FilterProductHandler
    {
        private readonly IProductRepository _productRepository;
        private readonly IPromotionRepository _promotionRepository;
        private readonly IPromotionService _promotionService;
        private readonly IRedisCacheService _cacheService;
        private readonly IMapper _mapper;

        public FilterProductHandler(
            IProductRepository productRepository,
            IPromotionRepository promotionRepository,
            IPromotionService promotionService,
            IRedisCacheService cacheService,
            IMapper mapper)
        {
            _productRepository = productRepository;
            _promotionRepository = promotionRepository;
            _promotionService = promotionService;
            _cacheService = cacheService;
            _mapper = mapper;
        }

        public async Task<List<ProductListResponse>> Handle(string categoryName)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
            {
                return new List<ProductListResponse>(); // Trả danh sách rỗng nếu tên danh mục không hợp lệ
            }

            // 🔑 Cache key
            string instanceName = "ProductInstance";
            string cacheKey = $"{instanceName}:products:filter:{categoryName.ToLower()}";

            // 🧠 1. Kiểm tra cache
            var cachedData = await _cacheService.GetCacheAsync<List<ProductListResponse>>(cacheKey);
            if (cachedData != null)
            {
                return cachedData;
            }

            // 🗃️ 2. Lấy từ repository
            var products = await _productRepository.GetProductsByCategoryNameAsync(categoryName);
            if (products == null || !products.Any())
            {
                return new List<ProductListResponse>();
            }

            // 🔖 3. Lấy danh sách khuyến mãi
            var promotions = await _promotionRepository.GetActiveProductPromotionsAsync();

            // 🔄 4. Map sang DTO
            var productList = _mapper.Map<List<ProductListResponse>>(products);

            // 💰 5. Áp dụng khuyến mãi
            foreach (var product in productList)
            {
                _promotionService.ApplyPromotion(
                    product.ProductId,
                    product.Price,
                    promotions,
                    out var discountedPrice,
                    out var promotionTitle);

                product.DiscountedPrice = discountedPrice;
                product.PromotionTitle = promotionTitle;
            }

            // ✅ 6. Lưu vào cache 10 phút
            await _cacheService.SetCacheAsync(cacheKey, productList, TimeSpan.FromMinutes(10));

            // ✅ 7. Trả về danh sách
            return productList;
        }

    }


}
