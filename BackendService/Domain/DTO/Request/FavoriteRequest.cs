using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Request
{
	public class FavoriteRequest
	{
		public int AccountId { get; set; }
		public int ProductId { get; set; }
	}
}
