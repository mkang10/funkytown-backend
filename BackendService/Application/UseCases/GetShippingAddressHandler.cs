
using AutoMapper;
using Domain.DTO.Request;
using Domain.DTO.Response;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class GetShippingAddressHandler
    {
        private readonly IShippingAddressRepository _shippingAddressRepository;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IMapper _mapper;
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<GetShippingAddressHandler> _logger;
        public GetShippingAddressHandler(
            IShippingAddressRepository shippingAddressRepository,
            IRedisCacheService redisCacheService,
            IMapper mapper,
            IOrderRepository orderRepository,
            ILogger<GetShippingAddressHandler> logger)
        {
            _shippingAddressRepository = shippingAddressRepository;
            _redisCacheService = redisCacheService;
            _mapper = mapper;
            _orderRepository = orderRepository;
            _logger = logger;
        }

        // Hàm tạo key cho cache dựa trên ShippingAddressId
        private string GetCacheKey(int shippingAddressId) => $"shippingaddress:{shippingAddressId}";
        private string GetAllAddressesCacheKey(int accountId) => $"shippingaddresses:account:{accountId}";
        /// <summary>
        /// Lấy thông tin địa chỉ giao hàng cho tài khoản từ cache, nếu không có thì lấy từ DB và cập nhật cache.
        /// </summary>
        /// <param name="shippingAddressId">ID của địa chỉ giao hàng</param>
        /// <param name="accountId">ID của tài khoản để kiểm tra tính hợp lệ</param>
        /// <returns>ShippingAddress nếu tồn tại và thuộc về tài khoản, ngược lại trả về null.</returns>
        public async Task<ShippingAddress?> HandleAsync(int shippingAddressId, int accountId)
        {

            var cacheKey = GetCacheKey(shippingAddressId);

            // Kiểm tra dữ liệu trên Redis
            var cachedAddress = await _redisCacheService.GetCacheAsync<ShippingAddress>(cacheKey);
            if (cachedAddress != null)
            {
                // Kiểm tra xem địa chỉ có thuộc tài khoản yêu cầu không
                if (cachedAddress.AccountId == accountId)
                    return cachedAddress;
                else
                    return null;
            }

            // Nếu không có trong cache, lấy từ Database
            var shippingAddress = await _shippingAddressRepository.GetByIdAsync(shippingAddressId);
            if (shippingAddress != null && shippingAddress.AccountId == accountId)
            {
                // Lưu vào cache với thời gian hết hạn (ví dụ: 1 giờ)
                await _redisCacheService.SetCacheAsync(cacheKey, shippingAddress, TimeSpan.FromHours(1));
            }

            return shippingAddress;
        }

        /// <summary>
        /// Lấy danh sách tất cả địa chỉ giao hàng của tài khoản.
        /// </summary>
        /// <param name="accountId">ID tài khoản</param>
        /// <returns>Danh sách địa chỉ giao hàng</returns>
        public async Task<List<ShippingAddress>> GetAllByAccountIdAsync(int accountId)
        {
            var cacheKey = GetAllAddressesCacheKey(accountId);

            
            var cachedList = await _redisCacheService.GetCacheAsync<List<ShippingAddress>>(cacheKey);
            if (cachedList != null)
                return cachedList;

            
            var addresses = await _shippingAddressRepository.GetShippingAddressesByAccountIdAsync(accountId);

            if (addresses != null && addresses.Any())
            {
                await _redisCacheService.SetCacheAsync(cacheKey, addresses, TimeSpan.FromHours(1));
            }

            return addresses ?? new List<ShippingAddress>();
        }


        public async Task<ResponseDTO<ShippingAddressResponse>> CreateShippingAddressHandler(CreateShippingAddressRequest request)
        {
            // Nếu địa chỉ mới được đánh dấu là mặc định
            if (request.IsDefault == true)
            {
                await EnsureOnlyOneDefaultAddressAsync(request.AccountId, null); // vì chưa có AddressId
            }

            var newAddress = _mapper.Map<ShippingAddress>(request);
            newAddress.IsDefault = request.IsDefault ?? false;

            await _shippingAddressRepository.CreateAsync(newAddress);

            var responseDto = _mapper.Map<ShippingAddressResponse>(newAddress);
            return new ResponseDTO<ShippingAddressResponse>(
                data: responseDto,
                status: true,
                message: "Created successfully"
            );
        }
        public async Task<ResponseDTO<ShippingAddress>> UpdateShippingAddressHandler(int id, UpdateShippingAddressRequest request)
        {
            var existing = await _shippingAddressRepository.GetByIdAsync(id);
            if (existing == null)
            {
                return new ResponseDTO<ShippingAddress>(null, true, "Không có địa chỉ đó tồn tại");
            }

            // Nếu request yêu cầu đặt làm mặc định
            if (request.IsDefault == true)
            {
                await EnsureOnlyOneDefaultAddressAsync(existing.AccountId, existing.AddressId);
            }

            // Cập nhật dữ liệu từ request → entity
            _mapper.Map(request, existing);

            await _shippingAddressRepository.UpdateAsync(existing);

            return new ResponseDTO<ShippingAddress>(existing, true, "Cập nhật địa chỉ thành công");
        }

        public async Task<ResponseDTO> DeleteShippingAddressHandler(int shippingAddressId)
        {
            var existing = await _shippingAddressRepository.GetByIdAsync(shippingAddressId);
            if (existing == null)
            {
                return new ResponseDTO(true, "Không có địa chỉ đó tồn tại");
            }

            // Tìm các đơn hàng đang dùng địa chỉ
            var relatedOrders = await _orderRepository.GetOrdersByShippingAddressId(shippingAddressId);

            foreach (var order in relatedOrders)
            {
                order.ShippingAddressId = null;
            }

            await _orderRepository.UpdateRangeAsync(relatedOrders);

            // Xóa địa chỉ
            await _shippingAddressRepository.DeleteAsync(existing);

            _logger.LogInformation("Đã xóa địa chỉ ID {ShippingAddressId} và cập nhật {Count} đơn hàng", shippingAddressId, relatedOrders.Count);

            return new ResponseDTO(true, "Xóa địa chỉ thành công và cập nhật các đơn hàng liên quan.");
        }
        /// <summary>
        /// Hủy trạng thái mặc định của địa chỉ cũ nếu tồn tại
        /// </summary>
        private async Task EnsureOnlyOneDefaultAddressAsync(int accountId, int? currentAddressId)
        {
            var existingDefault = await _shippingAddressRepository.GetDefaultAddressByAccountIdAsync(accountId);

            // Chỉ bỏ mặc định nếu địa chỉ mặc định hiện tại không phải là địa chỉ đang cập nhật
            if (existingDefault != null && existingDefault.AddressId != currentAddressId)
            {
                existingDefault.IsDefault = false;
                await _shippingAddressRepository.UpdateAsync(existingDefault);
            }
        }
    }
}
