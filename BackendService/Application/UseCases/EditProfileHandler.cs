using Application.Interfaces;
using AutoMapper;
using Domain.DTO.Request;
using Domain.DTO.Response;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
	public class EditProfileHandler
	{
		private readonly ICustomerProfileDataService _customerProfileDataService;
		private readonly IProfileRepository _profileRepository;
		private readonly IMapper _mapper;
        private readonly ICloudinaryService _cloudinaryService;
        public EditProfileHandler(
			ICustomerProfileDataService customerProfileDataService,
			IProfileRepository profileRepository,
			IMapper mapper,
            ICloudinaryService cloudinaryService)
		{
			_customerProfileDataService = customerProfileDataService;
			_profileRepository = profileRepository;
			_mapper = mapper;
            _cloudinaryService = cloudinaryService;
        }

        public async Task<EditProfileResponse> EditProfile(int accountId, EditProfileRequest request)
        {
            var (account, customerDetail) = await _customerProfileDataService.GetAccountAndDetailAsync(accountId);
            if (account == null)
            {
                return new EditProfileResponse { Success = false, Message = "Account not found" };
            }

            if (customerDetail == null)
            {
                return new EditProfileResponse { Success = false, Message = "Customer details not found" };
            }

            // Ánh xạ thông tin cơ bản
            _mapper.Map(request, account);
            _mapper.Map(request, customerDetail);

            // Upload ảnh nếu có
            if (request.AvatarImage != null && request.AvatarImage.Length > 0)
            {
                string imageUrl = await _cloudinaryService.UploadMediaAsync(request.AvatarImage);
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    account.ImagePath = imageUrl; // tuỳ bạn lưu Avatar ở bảng Account hay CustomerDetail
                }
            }

            await _profileRepository.UpdateAccountAsync(account);
            await _profileRepository.UpdateCustomerDetailAsync(customerDetail);

            return new EditProfileResponse { Success = true, Message = "Profile updated successfully" };
        }
    }


}
