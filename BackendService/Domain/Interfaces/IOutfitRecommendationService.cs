using Domain.DTO.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IOutfitRecommendationService
    {
        /// <summary>
        /// Gợi ý một set đồ gồm Top, Bottom, Shoes, Accessory dựa trên yêu cầu.
        /// </summary>
        Task<OutfitSuggestionDto> RecommendOutfitAsync(
            string occasion,
            string style,
            string colorPreference,
            int sizeId,
            CancellationToken ct = default);
    }
}
