using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Domain.DTO.Response
{
    public class CartWrapper
    {
        [JsonPropertyName("$values")]
        public List<CartItem> Values { get; set; } = new();
    }
}
