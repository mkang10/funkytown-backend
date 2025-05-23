using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class ShoppingCart
{
    public int CartId { get; set; }

    public int AccountId { get; set; }

    public DateTime? CreatedDate { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
}
