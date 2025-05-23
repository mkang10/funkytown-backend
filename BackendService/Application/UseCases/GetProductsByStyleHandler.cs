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
    public class GetProductsByStyleHandler
    {
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;
        private readonly IPromotionRepository _promotionRepository;
        private readonly IPromotionService _promotionService;

        public GetProductsByStyleHandler(
            IProductRepository productRepository,
            IMapper mapper,
            IPromotionRepository promotionRepository,
            IPromotionService promotionService)
        {
            _productRepository = productRepository;
            _mapper = mapper;
            _promotionRepository = promotionRepository;
            _promotionService = promotionService;
        }

        public async Task<List<ProductListResponse>> HandleAsync(string styleName, int page, int pageSize)
        {
            var products = await _productRepository.GetProductsByStyleNameAsync(styleName, page, pageSize);

            if (products == null || !products.Any())
                return new List<ProductListResponse>();

            var promotions = await _promotionRepository.GetActiveProductPromotionsAsync();

            var productList = _mapper.Map<List<ProductListResponse>>(products);

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

            return productList;
        }
    }

}
