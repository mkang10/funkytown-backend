using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Response
{
    public class ShoppingCartResponseDto
    {
        public List<CartItemResponse> CartItems { get; set; } = new();
    }
}
