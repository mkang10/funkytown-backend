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
    public class SuggestProductsHandler
    {
        private readonly IProfileRepository _profileRepository;
        private readonly IInventoryServiceClient _inventoryServiceClient;
        private readonly IMapper _mapper;
        private readonly ICustomerRecentClickService _customerRecentClickService;

        public SuggestProductsHandler(IProfileRepository profileRepository, IInventoryServiceClient inventoryServiceClient, IMapper mapper,ICustomerRecentClickService customerRecentClickService)
        {
            _profileRepository = profileRepository;
            _inventoryServiceClient = inventoryServiceClient;
            _mapper = mapper;
            _customerRecentClickService = customerRecentClickService;
        }

        public async Task<List<SuggestedProductResponse>> HandleAsync(int accountId, int page = 1, int pageSize = 10)
        {
            var customerDetail = await _profileRepository.GetCustomerDetailByAccountIdAsync(accountId);
            if (customerDetail == null)
            {
                // Bạn có thể throw exception hoặc return empty list
                return new List<SuggestedProductResponse>();
            }

            var customerDetailId = customerDetail.CustomerDetailId;

            var resultProducts = new List<ProductResponse>();
            var usedStyleIds = new HashSet<int>();

            // 🔥 B1: Check hành vi recent click
            var recentClicks = await _customerRecentClickService.GetRecentClicksAsync(customerDetailId);

            if (recentClicks != null && recentClicks.Any())
            {
                var topRecent = recentClicks.OrderByDescending(x => x.Value).FirstOrDefault();

                if (topRecent.Value >= 2)
                {
                    var style = await _profileRepository.GetStyleByIdAsync(topRecent.Key);

                    if (style != null)
                    {
                        var products = await _inventoryServiceClient.GetProductsByStyleNameAsync(style.StyleName, 1, pageSize);

                        if (products != null && products.Any())
                        {
                            resultProducts.AddRange(products);
                            usedStyleIds.Add(style.StyleId);
                        }
                    }
                }
            }

            // 🔥 B2: Nếu chưa đủ --> fallback theo Point
            if (resultProducts.Count < pageSize)
            {
                var styles = await _profileRepository.GetStylesByCustomerDetailIdAsync(customerDetailId);
                if (styles != null && styles.Any())
                {
                    foreach (var styleItem in styles.OrderByDescending(s => s.Point))
                    {
                        if (usedStyleIds.Contains(styleItem.StyleId)) continue; // 🔥 Bỏ qua style đã dùng trước đó

                        var products = await _inventoryServiceClient.GetProductsByStyleNameAsync(styleItem.Style.StyleName, 1, pageSize);

                        if (products != null && products.Any())
                        {
                            foreach (var product in products)
                            {
                                if (resultProducts.Count >= pageSize) break;
                                resultProducts.Add(product);
                            }
                        }

                        if (resultProducts.Count >= pageSize) break;
                    }
                }
            }

            // 🔥 B3: Mapping ra DTO
            return _mapper.Map<List<SuggestedProductResponse>>(resultProducts.Take(pageSize));
        }



    }
}
