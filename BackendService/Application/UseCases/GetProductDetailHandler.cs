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
using static Domain.DTO.Response.ProductDetailDTO;

namespace Application.UseCases
{
    public class GetProductDetailHandler
    {
        private readonly IProductVarRepos _variantRepo;
        private readonly IProductRepository _productRepository;
        private readonly IRedisCacheService _cacheService;
        private readonly IMapper _mapper;
        private readonly IPromotionRepository _promotionRepository;
        private readonly IWareHousesStockRepository _wareHousesStockRepository;
		private readonly IPromotionService _promotionService;
		public GetProductDetailHandler(IProductRepository productRepository,
                                       IRedisCacheService cacheService, 
                                       IMapper mapper,
                                       IPromotionRepository promotionRepository,
                                       IWareHousesStockRepository wareHousesStockRepository,
                                       IPromotionService promotionService,
                                       IProductVarRepos variantRepo)
        {
            _productRepository = productRepository;
            _variantRepo = variantRepo;
            _cacheService = cacheService;
            _mapper = mapper;
            _promotionRepository = promotionRepository;
            _wareHousesStockRepository = wareHousesStockRepository;
            _promotionService = promotionService;
        }

        public async Task<ProductDetailResponseInven?> Handle(int productId, int? accountId = null)
        {
            string cacheKey = $"ProductInstance:product:{productId}";

            // 1. Try cache
            var cached = await _cacheService.GetCacheAsync<ProductDetailResponseInven>(cacheKey);
            if (cached != null)
            {
                if (accountId.HasValue)
                    cached.IsFavorite = await _productRepository.IsProductFavoriteAsync(accountId.Value, productId);
                return cached;
            }

            // 2. Lấy product (không quan tâm variants)
            var product = await _productRepository.GetProductByIdAsync(productId);
            if (product == null) return null;

            var promotions = await _promotionRepository.GetActiveProductPromotionsAsync();

            // 3. Map phần chung
            var productDetail = _mapper.Map<ProductDetailResponseInven>(product);

            // 4. Lấy riêng variants đã publish
            var publishedVariants = await _productRepository
                .GetPublishedVariantsByProductIdAsync(productId);

            // 5. Map sang DTO
            var variantDtos = _mapper.Map<List<ProductVariantResponse>>(publishedVariants);

            // 6. Áp khuyến mãi
            foreach (var v in variantDtos)
            {
                _promotionService.ApplyPromotion(
                    productId,
                    v.Price,
                    promotions,
                    out var discountedPrice,
                    out var promotionTitle);

                v.DiscountedPrice = discountedPrice;
                v.PromotionTitle = promotionTitle;
            }

            // 7. Gán lại vào response
            productDetail.Variants = variantDtos;

            // 8. Xử lý favorite
            productDetail.IsFavorite = false;
            if (accountId.HasValue)
                productDetail.IsFavorite = await _productRepository.IsProductFavoriteAsync(accountId.Value, productId);

            // 9. Lưu cache và return
            await _cacheService.SetCacheAsync(cacheKey, productDetail, TimeSpan.FromMinutes(30));
            return productDetail;
        }

        public async Task<ResponseDTO<ProductWithVariantsDto>> GetProductWithVariantsAsync(int productId)
        {
            var product = await _productRepository.GetByIdWithVariantsAsync(productId);
            if (product == null)
                return new ResponseDTO<ProductWithVariantsDto>(null, false, "Không tìm thấy sản phẩm");

            var variantIds = await _variantRepo.GetAllVariantIdsByProductIdAsync(productId);
            var productDto = _mapper.Map<ProductDto>(product);

            // Lấy image chính (IsMain) giống GetAllProductsAsync
            productDto.ImagePath = product.ProductImages
                                        .FirstOrDefault(pi => pi.IsMain)
                                        ?.ImagePath;
            productDto.Image = product.ProductImages
     .Select(pi => new ProductDto.Images
     {
         ProductImageId = pi.ProductImageId,
         ImagePath = pi.ImagePath,
         IsMain = pi.IsMain
     }).ToList();

            var variantDtos = _mapper.Map<List<ProductVariantDto>>(product.ProductVariants);

            var dto = new ProductWithVariantsDto
            {
                Product = productDto,
                VariantIds = variantIds,
                Variants = variantDtos
            };

            return new ResponseDTO<ProductWithVariantsDto>(dto, true, "Thành công");
        }



    }

}
