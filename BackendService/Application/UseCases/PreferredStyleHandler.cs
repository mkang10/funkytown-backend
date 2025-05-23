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
    public class PreferredStyleHandler 
    {
        private readonly IProfileRepository _profileRepository;
        private readonly IMapper _mapper;

        public PreferredStyleHandler(IProfileRepository profileRepository, IMapper mapper)
        {
            _profileRepository = profileRepository;
            _mapper = mapper;
        }

        public async Task<List<StyleResponse>> GetPreferredStylesByAccountIdAsync(int accountId)
        {
            // 🔥 B1: Tìm CustomerDetail từ AccountId
            var customerDetail = await _profileRepository.GetCustomerDetailByAccountIdAsync(accountId);
            if (customerDetail == null)
            {
                return new List<StyleResponse>();
            }

            // 🔥 B2: Lấy toàn bộ Style trong hệ thống
            var allStyles = await _profileRepository.GetAllStylesAsync();

            // 🔥 B3: Lấy danh sách Style yêu thích của CustomerDetailId
            var preferredStyles = await _profileRepository.GetPreferredStylesByCustomerDetailIdAsync(customerDetail.CustomerDetailId);
            var preferredStyleIds = preferredStyles.Select(cs => cs.StyleId).ToHashSet();

            // 🔥 B4: Map toàn bộ Style + Đánh dấu IsSelected
            var result = allStyles.Select(style => new StyleResponse
            {
                StyleId = style.StyleId,
                StyleName = style.StyleName,
                IsSelected = preferredStyleIds.Contains(style.StyleId)
            }).ToList();

            return result;
        }
    
        public async Task<ResponseDTO> UpdatePreferredStylesAsync(int accountId, List<int> styleIds)
        {
            var customerDetail = await _profileRepository.GetCustomerDetailByAccountIdAsync(accountId);
            if (customerDetail == null)
            {
                return new ResponseDTO(false, "Không tìm thấy thông tin Customer.");
            }

            await _profileRepository.UpdatePreferredStylesAsync(customerDetail.CustomerDetailId, styleIds);

            return new ResponseDTO(true, "Cập nhật danh sách style yêu thích thành công.");
        }

    }
}
