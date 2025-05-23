using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IPromotionRepository
    {
        Task<List<Promotion>> GetActiveProductPromotionsAsync();

        Task<int> CreatePromotionAsync(Promotion promotion);
        Task<bool> UpdatePromotionAsync(Promotion promotion);
        Task<bool> DeletePromotionAsync(int promotionId);
        Task<List<Promotion>> GetAllPromotionsAsync(string? status);
        Task<Promotion?> GetPromotionByIdAsync(int promotionId);
    }
}
