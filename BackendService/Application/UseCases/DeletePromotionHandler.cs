using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class DeletePromotionHandler
    {
        private readonly IPromotionRepository _promotionRepository;

        public DeletePromotionHandler(IPromotionRepository promotionRepository)
        {
            _promotionRepository = promotionRepository;
        }

        public async Task<bool> Handle(int promotionId)
        {
            return await _promotionRepository.DeletePromotionAsync(promotionId);
        }
    }

}
