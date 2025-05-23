using Application.Interfaces;
using Domain.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
	public class PromotionService : IPromotionService
	{
		public void ApplyPromotion(
			int productId,
			decimal originalPrice,
			List<Promotion> activePromotions,
			out decimal discountedPrice,
			out string? promotionTitle)
		{
			discountedPrice = originalPrice;
			promotionTitle = null;

			var applicablePromotion = activePromotions.FirstOrDefault(p =>
				!string.IsNullOrEmpty(p.ApplyValue) &&
				JsonConvert.DeserializeObject<List<int>>(p.ApplyValue).Contains(productId));

			if (applicablePromotion != null)
			{
				if (applicablePromotion.DiscountType == "PERCENTAGE")
				{
					var discount = (originalPrice * applicablePromotion.DiscountValue) / 100;
					discountedPrice = Math.Max(originalPrice - discount, 0);
				}
				else if (applicablePromotion.DiscountType == "FIXED_AMOUNT")
				{
					discountedPrice = Math.Max(originalPrice - applicablePromotion.DiscountValue, 0);
				}

				promotionTitle = applicablePromotion.Title;
			}
		}
	}

}
