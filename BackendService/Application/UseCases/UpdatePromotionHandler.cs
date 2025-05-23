using Domain.DTO.Request;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class UpdatePromotionHandler
    {
        private readonly IPromotionRepository _promotionRepository;

        public UpdatePromotionHandler(IPromotionRepository promotionRepository)
        {
            _promotionRepository = promotionRepository;
        }

        /// <summary>
        /// Cập nhật thông tin khuyến mãi
        /// </summary>
        public async Task<bool> UpdatePromotionAsync(int promotionId, UpdatePromotionRequest request)
        {
            var promotion = await _promotionRepository.GetPromotionByIdAsync(promotionId);
            if (promotion == null) return false;

            // Cập nhật các thông tin nếu có giá trị
            if (!string.IsNullOrEmpty(request.Title))
                promotion.Title = request.Title;
            if (request.DiscountValue.HasValue)
                promotion.DiscountValue = request.DiscountValue.Value;
            if (request.EndDate.HasValue)
                promotion.EndDate = request.EndDate.Value;
            if (!string.IsNullOrEmpty(request.Status))
                promotion.Status = request.Status; // Nếu cần cập nhật luôn trạng thái

            return await _promotionRepository.UpdatePromotionAsync(promotion);
        }

        /// <summary>
        /// Chỉ cập nhật trạng thái khuyến mãi
        /// </summary>
        public async Task<bool> UpdatePromotionStatusAsync(int promotionId, UpdatePromotionStatusRequest request)
        {
            var promotion = await _promotionRepository.GetPromotionByIdAsync(promotionId);
            if (promotion == null) return false;

            promotion.Status = request.Status;
            return await _promotionRepository.UpdatePromotionAsync(promotion);
        }
    }

}
