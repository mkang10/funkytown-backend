using System.Collections.Generic;

namespace Domain.DTO.Request
{
    public class OutfitRequest
    {
        /// <summary>
        /// Chiều cao (cm)
        /// </summary>
        public int? HeightCm { get; set; }

        /// <summary>
        /// Cân nặng (kg)
        /// </summary>
        public int? WeightKg { get; set; }

        /// <summary>
        /// Dịp mặc (tiệc, dạo phố, ...)
        /// </summary>
        public string? Occasion { get; set; }

        /// <summary>
        /// Phong cách (Street, Formal, ...)
        /// </summary>
        public string? Style { get; set; }

        /// <summary>
        /// Danh sách màu ưa thích
        /// </summary>
        public List<string>? ColorPreferences { get; set; }

        /// <summary>
        /// SizeId (nếu đã xác định)
        /// </summary>
        public int? SizeId { get; set; }
    }
}
