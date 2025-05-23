
using Application.Interfaces;
using AutoMapper;
using Azure.Core;
using Domain.DTO.Request;
using Domain.DTO.Response;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class CheckOutHandler
    {
        private readonly IDistributedCache _cache;
        private readonly IShippingAddressRepository _shippingAddressRepository;
        private readonly GetSelectedCartItemsHandler _getSelectedCartItemsHandler;
        private readonly IConfiguration _configuration;
        private readonly ShippingCostHandler _shippingCostHandler;

        public CheckOutHandler(
            IDistributedCache cache,
            IShippingAddressRepository shippingAddressRepository,
            GetSelectedCartItemsHandler getSelectedCartItemsHandler,
            ShippingCostHandler shippingCostHandler,
            IConfiguration configuration)
        {
            _cache = cache;
            _shippingAddressRepository = shippingAddressRepository;
            _getSelectedCartItemsHandler = getSelectedCartItemsHandler;
            _shippingCostHandler = shippingCostHandler;
            _configuration = configuration;
        }

        public async Task<CheckOutResponse?> Handle(CheckOutRequest request)
        {
            var checkOutSessionId = Guid.NewGuid().ToString(); // Tạo Session ID

            // Lấy địa chỉ giao hàng mặc định và tất cả địa chỉ của account
            var shippingAddresses = await _shippingAddressRepository.GetShippingAddressesByAccountIdAsync(request.AccountId);
            var defaultAddress = await _shippingAddressRepository.GetDefaultShippingAddressAsync(request.AccountId);

            // Lấy danh sách sản phẩm đã chọn từ giỏ hàng
            var orderItems = await _getSelectedCartItemsHandler.Handle(request.AccountId, request.SelectedProductVariantIds);
            if (orderItems == null || !orderItems.Any()) return null;

            // Tính tổng tiền sản phẩm
            var subTotal = orderItems.Sum(item => item.DiscountedPrice * item.Quantity);
            if (subTotal <= 0) return null;

            // Tính phí vận chuyển nếu có địa chỉ mặc định, ngược lại gán = 0
            int flatShippingCost = int.TryParse(_configuration["Shipping:FlatRate"], out var value) ? value : 30000;

            decimal shippingCost = flatShippingCost;
            int? shippingAddressId = defaultAddress?.AddressId;

            var availablePaymentMethods = new List<string> { "COD", "PAYOS" };

            // Lưu dữ liệu vào Redis
            var checkOutData = new CheckOutData
            {
                AccountId = request.AccountId,
                SubTotal = subTotal,
                ShippingCost = shippingCost,
                ShippingAddressId = shippingAddressId,
                Items = orderItems
            };

            var cacheKey = $"checkout:{checkOutSessionId}";
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15)
            };
            await _cache.SetStringAsync(cacheKey, JsonConvert.SerializeObject(checkOutData), cacheOptions);

            return new CheckOutResponse
            {
                CheckOutSessionId = checkOutSessionId,
                SubTotal = subTotal,
                ShippingCost = shippingCost,
                AvailablePaymentMethods = availablePaymentMethods,
                ShippingAddress = defaultAddress, // Có thể null
                ShippingAddresses = shippingAddresses ?? new List<ShippingAddress>(), // Tránh null
                Items = orderItems.Select(item => new OrderItemResponse
                {
                    ProductVariantId = item.ProductVariantId,
                    ProductName = item.ProductName,
                    ImageUrl = item.ImageUrl,
                    Color = item.Color,
                    Size = item.Size,
                    Quantity = item.Quantity,
                    Price = item.Price,
                    PriceAtPurchase = item.DiscountedPrice,
                    DiscountApplied = item.Price - item.DiscountedPrice
                }).ToList()
            };
        }


    }
}
