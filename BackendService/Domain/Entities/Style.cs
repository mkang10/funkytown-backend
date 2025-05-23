using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class Style
{
    public int StyleId { get; set; }

    public string StyleName { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual ICollection<CustomerStyle> CustomerStyles { get; set; } = new List<CustomerStyle>();
}
