using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Response
{
    public class InvoiceForEmailDTO
    {
        public int OrderId { get; set; }

        public int AccountId { get; set; }

        public int? WareHouseId { get; set; }

        public int? ShippingAddressId { get; set; }

        public DateTime? CreatedDate { get; set; }

        public string? Status { get; set; }

        public decimal? OrderTotal { get; set; }

        public decimal? ShippingCost { get; set; }

        public string? DeliveryMethod { get; set; }

        public string FullName { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string PhoneNumber { get; set; } = null!;

        public string Address { get; set; } = null!;

        public string City { get; set; } = null!;

        public string District { get; set; } = null!;

        public string? Country { get; set; }

        public string? Province { get; set; }

        public List<OrderDetailEmailDTO> OrderdetailEmail { get; set; } = new();

    }
    public class OrderDetailEmailDTO
    {
        public int ProductVariantId { get; set; }

        public int Quantity { get; set; }

        public decimal PriceAtPurchase { get; set; }

        public decimal? DiscountApplied { get; set; }
        public ProductDetailEmailDTO Item { get; set; } 

    }
    public class ProductDetailEmailDTO
    {
        public string ProductId { get; set; }
            
        public string? SizeId { get; set; }

        public string? ColorId { get; set; }

        public decimal Price { get; set; }

        public string? ImagePath { get; set; }

        public string? Sku { get; set; }
    }


}
