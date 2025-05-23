
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
	public class AddFavoriteHandler
	{
		private readonly IProductRepository _productRepository;
		private readonly IRedisCacheService _cacheService;
		private readonly IMapper _mapper;

		public AddFavoriteHandler(
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

			// Kiểm tra cache trang 1
			var cached = await _cacheService.GetCacheAsync<List<ProductListResponse>>(cacheKeyPage1);
			Product? product = null;

			if (cached != null)
			{
				//Lấy chi tiết sản phẩm để thêm vào cache
				product = await _productRepository.GetProductByIdAsync(request.ProductId);
				if (product != null)
				{
					var dto = _mapper.Map<ProductListResponse>(product);
					dto.IsFavorite = true;
					cached.Insert(0, dto);
					if (cached.Count > 10) cached = cached.Take(10).ToList();
					await _cacheService.SetCacheAsync(cacheKeyPage1, cached, TimeSpan.FromMinutes(10));
				}
			}

			// Ghi xuống DB
			await _productRepository.AddFavoriteAsync(request.AccountId, request.ProductId);

			// Nếu cache ban đầu chưa có → cần lấy sản phẩm để trả về
			if (product == null)
			{
				product = await _productRepository.GetProductByIdAsync(request.ProductId);
			}

			// Xoá cache phân trang để đồng bộ lại sau
			string cachePattern = $"ProductInstance:favorites:view:account:{request.AccountId}:*";
			await _cacheService.RemoveByPatternAsync(cachePattern);

			// Trả về thông tin sản phẩm
			if (product == null) return null;

			var result = _mapper.Map<ProductListResponse>(product);
			result.IsFavorite = true;
			return result;
		}
	}

}
