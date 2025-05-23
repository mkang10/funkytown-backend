using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class InteractionHandler
    {
        private readonly IProfileRepository _profileRepository;
        private readonly IInventoryServiceClient _inventoryServiceClient;
        private readonly ICustomerRecentClickService _customerRecentClickService;
        public InteractionHandler(IProfileRepository profileRepository, IInventoryServiceClient inventoryServiceClient,ICustomerRecentClickService customerRecentClickService)
        {
            _profileRepository = profileRepository;
            _inventoryServiceClient = inventoryServiceClient;
            _customerRecentClickService = customerRecentClickService;
        }

        public async Task HandleAsync(int accountId, int productId)
        {
            // 🔥 Bước 1: Lookup CustomerDetailId từ AccountId
            var customerDetail = await _profileRepository.GetCustomerDetailByAccountIdAsync(accountId);
            if (customerDetail == null)
            {
                throw new Exception($"Không tìm thấy thông tin CustomerDetail cho AccountId = {accountId}");
            }

            var customerDetailId = customerDetail.CustomerDetailId;

            // 🔥 Bước 2: Thực hiện như logic cũ
            var product = await _inventoryServiceClient.GetProductByIdAsync(productId);

            var styleName = product.Style;

            var style = await _profileRepository.GetStyleByNameAsync(styleName);

            if (style == null)
            {
                throw new Exception($"Không tìm thấy style {styleName} trong hệ thống");
            }

            var styleId = style.StyleId;

            await _customerRecentClickService.IncreaseStyleClickAsync(customerDetailId, styleId);

            var customerStyle = await _profileRepository.GetCustomerStyleAsync(customerDetailId, styleId);

            if (customerStyle != null)
            {
                customerStyle.ClickCount++;
                customerStyle.Point += 2; // 🔥 Mỗi lần click cộng thêm 2 điểm
                customerStyle.LastUpdatedDate = DateTime.UtcNow;

                await _profileRepository.UpdateAsync(customerStyle);
            }
            else
            {
                await _profileRepository.InsertAsync(new CustomerStyle
                {
                    CustomerDetailId = customerDetailId,
                    StyleId = styleId,
                    Point = 2,
                    ClickCount = 1,
                    IsFromPreference = false,
                    CreatedDate = DateTime.UtcNow,
                    LastUpdatedDate = DateTime.UtcNow
                });
            }
        }

    }

}
