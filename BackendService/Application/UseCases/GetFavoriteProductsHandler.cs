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
	public class GetFavoriteProductsHandler
	{
		private readonly IProductRepository _productRepository;
		private readonly IRedisCacheService _cacheService;
		private readonly IMapper _mapper;
		private readonly IPromotionRepository _promotionRepository;
		private readonly IPromotionService _promotionService;
		public GetFavoriteProductsHandler(IProductRepository productRepository, IMapper mapper, IRedisCacheService cacheService, IPromotionRepository promotionRepository, IPromotionService promotionService)
		{
			_productRepository = productRepository;
			_mapper = mapper;
			_cacheService = cacheService;
			_promotionRepository = promotionRepository;
			_promotionService = promotionService;
		}

		public async Task<List<ProductListResponse>> Handle(int accountId, int page, int pageSize)
		{
			string instanceName = "ProductInstance";
			string cacheKey = $"{instanceName}:favorites:view:account:{accountId}:page:{page}:size:{pageSize}";

			var cached = await _cacheService.GetCacheAsync<List<ProductListResponse>>(cacheKey);
			if (cached != null)
				return cached;

			// ❌ Nếu cache miss, truy vấn trực tiếp danh sách sản phẩm yêu thích
			var products = await _productRepository.GetFavoritePagedProductsAsync(accountId, page, pageSize);

			if (products == null || !products.Any())
				return new List<ProductListResponse>();

			// Có thể giữ phần khuyến mãi nếu bạn muốn hiển thị giá giảm
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
				product.IsFavorite = true;
			}

			await _cacheService.SetCacheAsync(cacheKey, productList, TimeSpan.FromMinutes(10));

			return productList;
		}

	}
}
