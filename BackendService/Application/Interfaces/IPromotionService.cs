using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
	public interface IPromotionService
	{
		void ApplyPromotion(
			int productId,
			decimal originalPrice,
			List<Promotion> activePromotions,
			out decimal discountedPrice,
			out string? promotionTitle);
	}

}
