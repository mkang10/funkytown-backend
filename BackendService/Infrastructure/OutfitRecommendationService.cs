// Infrastructure/Services/OutfitRecommendationService.cs
using Domain.Entities;
using Domain.Interfaces;
using Domain.Models;
using Domain.DTO.Request;

namespace Infrastructure
{
    public class OutfitRecommendationService : IOutfitRecommendationService
    {
        private readonly IProductRepository _prodRepo;

        public OutfitRecommendationService(IProductRepository prodRepo)
        {
            _prodRepo = prodRepo;
        }

        public async Task<OutfitSuggestionDto> RecommendOutfitAsync(
            string occasion,
            string style,
            string colorPreference,
            int sizeId,
            CancellationToken ct = default)
        {
            var variants = await _prodRepo.GetVariantsByFiltersAsync(
                occasion, style, sizeId, ct);

            // Phân nhóm theo category
            var tops = variants.Where(v => v.Product.Category?.Name == "Top").ToList();
            var bottoms = variants.Where(v => v.Product.Category?.Name == "Bottom").ToList();
            var shoes = variants.Where(v => v.Product.Category?.Name == "Shoes").ToList();
            var accessories = variants.Where(v => v.Product.Category?.Name == "Accessory").ToList();

            // Chọn Top: ưu tiên đúng màu khách thích
            var top = tops.FirstOrDefault(v =>
                        string.Equals(v.Color?.ColorName, colorPreference, StringComparison.OrdinalIgnoreCase))
                      ?? tops.FirstOrDefault();

            // Chọn Bottom: ưu tiên màu tương phản hoặc cùng tông
            var bottom = bottoms.FirstOrDefault(v =>
                        IsComplementaryColor(top?.Color?.ColorCode, v.Color?.ColorCode))
                         ?? bottoms.FirstOrDefault();

            // Chọn Shoes: cùng màu với top
            var shoesPick = shoes.FirstOrDefault(v =>
                        v.Color?.ColorName == top?.Color?.ColorName)
                         ?? shoes.FirstOrDefault();

            // Chọn Accessory: bất kỳ
            var accessory = accessories.FirstOrDefault();

            return new OutfitSuggestionDto
            {
                Top = top,
                Bottom = bottom,
                Shoes = shoesPick,
                Accessory = accessory
            };
        }

        /// <summary>
        /// Ví dụ: kiểm tra màu tương phản dựa trên mã hex (rất đơn giản).
        /// </summary>
        private bool IsComplementaryColor(string? hex1, string? hex2)
        {
            if (string.IsNullOrEmpty(hex1) || string.IsNullOrEmpty(hex2))
                return false;
            // Lấy độ sáng đơn giản: trung bình R,G,B
            int r1 = Convert.ToInt32(hex1.Substring(1, 2), 16);
            int g1 = Convert.ToInt32(hex1.Substring(3, 2), 16);
            int b1 = Convert.ToInt32(hex1.Substring(5, 2), 16);
            int lum1 = (r1 + g1 + b1) / 3;

            int r2 = Convert.ToInt32(hex2.Substring(1, 2), 16);
            int g2 = Convert.ToInt32(hex2.Substring(3, 2), 16);
            int b2 = Convert.ToInt32(hex2.Substring(5, 2), 16);
            int lum2 = (r2 + g2 + b2) / 3;

            // Nếu chênh lệch độ sáng > 60 thì coi là tương phản
            return Math.Abs(lum1 - lum2) > 60;
        }
    }
}
