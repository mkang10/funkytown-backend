using Domain.DTO.Response;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class GetAllPromotionsHandler
    {
        private readonly IPromotionRepository _promotionRepository;

        public GetAllPromotionsHandler(IPromotionRepository promotionRepository)
        {
            _promotionRepository = promotionRepository;
        }

        public async Task<List<PromotionResponse>> Handle(string? status)
        {
            var promotions = await _promotionRepository.GetAllPromotionsAsync(status);
            return promotions.Select(p => new PromotionResponse
            {
                PromotionId = p.PromotionId,
                Title = p.Title,
                Description =p.Description,
                DiscountType = p.DiscountType,
                DiscountValue = p.DiscountValue,
                MinOrderAmount = p.MinOrderAmount,
                MaxDiscountAmount = p.MaxDiscountAmount,
                ApplyTo = p.ApplyTo,
                ApplyValue = p.ApplyValue,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                Status = p.Status
            }).ToList();
        }
    }

}
