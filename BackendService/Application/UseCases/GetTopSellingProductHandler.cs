using Application.Interfaces;
using AutoMapper;
using Domain.DTO.Response;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class GetTopSellingProductHandler
    {
        private readonly IProductRepository _productRepository;
        private readonly ILogger<GetTopSellingProductHandler> _logger;
        private readonly IRedisCacheService _cacheService;
        private readonly IPromotionRepository _promotionRepository;
        private readonly IPromotionService _promotionService;
        private readonly IMapper _mapper;
        public GetTopSellingProductHandler(IProductRepository productRepository, 
                                           ILogger<GetTopSellingProductHandler> logger, 
                                           IRedisCacheService cacheService,
                                           IPromotionRepository promotionRepository,
                                           IPromotionService promotionService,
                                           IMapper mapper)
        {
            _productRepository = productRepository;
            _logger = logger;
            _cacheService = cacheService;
            _promotionRepository = promotionRepository;
            _promotionService = promotionService;
            _mapper = mapper;
        }

        public async Task<ResponseDTO<List<TopSellingProductResponse>>> GetTopSellingProductsAsync(
                                                                                    DateTime? from, DateTime? to, int top = 10)
        {
            string instanceName = "ProductInstance";
            string cacheKey = $"{instanceName}:products:top-selling:from:{from?.Date:yyyy-MM-dd}:to:{to?.Date:yyyy-MM-dd}:top:{top}";

            var cached = await _cacheService.GetCacheAsync<List<TopSellingProductResponse>>(cacheKey);
            if (cached != null)
                return new ResponseDTO<List<TopSellingProductResponse>>(cached, true, "Lấy top sản phẩm bán chạy thành công (cache)");

            var orders = await _productRepository.GetCompletedOrdersWithDetailsAsync(from, to);
            var orderDetails = orders.SelectMany(o => o.OrderDetails).ToList();

            var promotions = await _promotionRepository.GetActiveProductPromotionsAsync();

            var productGroups = orderDetails
            .GroupBy(od => od.ProductVariant.Product.ProductId)
            .Select(g =>
            {
                var first = g.First();
                var product = first.ProductVariant.Product;

                // ✅ Lấy toàn bộ biến thể (ProductVariants) từ các orderDetails
                var allVariants = g.Select(x => x.ProductVariant).Where(pv => pv != null).ToList();

                // ✅ Gom toàn bộ ColorCode từ các biến thể (tránh trùng)
                var colorCodes = allVariants
                    .Where(pv => pv.Color != null && !string.IsNullOrEmpty(pv.Color.ColorCode))
                    .Select(pv => pv.Color.ColorCode!)
                    .Distinct()
                    .ToList();

                // ⚡ Ánh xạ từ Product sang Response
                var response = _mapper.Map<TopSellingProductResponse>(product);

                response.QuantitySold = g.Sum(x => x.Quantity);
                response.Revenue = g.Sum(x => x.Quantity * x.PriceAtPurchase);
                response.Price = allVariants.First().Price;
                response.Colors = colorCodes; // ✅ Gán danh sách màu

                _promotionService.ApplyPromotion(
                    product.ProductId,
                    response.Price,
                    promotions,
                    out var discountedPrice,
                    out var promotionTitle);

                response.DiscountedPrice = discountedPrice;
                response.PromotionTitle = promotionTitle;

                return response;
            })
            .OrderByDescending(x => x.QuantitySold)
            .Take(top)
            .ToList();


            await _cacheService.SetCacheAsync(cacheKey, productGroups, TimeSpan.FromMinutes(10));

            return new ResponseDTO<List<TopSellingProductResponse>>(productGroups, true, "Lấy top sản phẩm bán chạy thành công");
        }



    }
}
