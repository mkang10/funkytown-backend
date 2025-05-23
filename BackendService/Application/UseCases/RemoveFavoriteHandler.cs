using AutoMapper;
using Domain.DTO.Request;
using Domain.DTO.Response;
using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
	public class RemoveFavoriteHandler
	{
		private readonly IProductRepository _productRepository;
		private readonly IRedisCacheService _cacheService;
		private readonly IMapper _mapper;

		public RemoveFavoriteHandler(
			IProductRepository productRepository,
			IRedisCacheService cacheService,
			IMapper mapper)
		{
			_productRepository = productRepository;
			_cacheService = cacheService;
			_mapper = mapper;
		}

		public async Task<ProductListResponse?> HandleAsync(FavoriteRequest request)
		{
			string cacheKeyPage1 = $"ProductInstance:favorites:view:account:{request.AccountId}:page:1:size:10";

			// 1️⃣ Kiểm tra cache trang 1
			var cached = await _cacheService.GetCacheAsync<List<ProductListResponse>>(cacheKeyPage1);
			Product? product = null;

			if (cached != null)
			{
				cached = cached.Where(p => p.ProductId != request.ProductId).ToList();
				await _cacheService.SetCacheAsync(cacheKeyPage1, cached, TimeSpan.FromMinutes(10));
			}

			// 2️⃣ Xoá khỏi DB
			await _productRepository.RemoveFavoriteAsync(request.AccountId, request.ProductId);

			// 3️⃣ Lấy sản phẩm để trả về
			product = await _productRepository.GetProductByIdAsync(request.ProductId);

			// 4️⃣ Xoá toàn bộ cache phân trang
			string cachePattern = $"ProductInstance:favorites:view:account:{request.AccountId}:*";
			await _cacheService.RemoveByPatternAsync(cachePattern);

			if (product == null) return null;

			var result = _mapper.Map<ProductListResponse>(product);
			result.IsFavorite = false;
			return result;
		}
	}

}
