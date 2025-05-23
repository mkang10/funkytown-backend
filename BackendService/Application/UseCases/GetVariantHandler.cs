using Domain.DTO.Response;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Application.UseCases.GetVariantHandler;

namespace Application.UseCases
{
    
        public class GetVariantHandler 
        {
            private readonly IProductVarRepos _repository;

            public GetVariantHandler(IProductVarRepos repository)
            {
                _repository = repository;
            }

            public async Task<ResponseDTO<ProductVariantDetailDTO>> GetProductVariantDetailAsync(int variantId)
            {
                var variant = await _repository.GetByIdWithDetailsAsync(variantId);
                if (variant == null)
                {
                    return new ResponseDTO<ProductVariantDetailDTO>(
                        data: null!,
                        status: false,
                        message: $"Product variant with ID {variantId} not found"
                    );
                }

                var dto = new ProductVariantDetailDTO
                {
                    VariantId = variant.VariantId,
                    ProductName = variant.Product.Name,
                    SizeName = variant.Size?.SizeName,
                    ColorName = variant.Color?.ColorName,
                    Price = variant.Price,
                    ImagePath = variant.ImagePath,
                    Sku = variant.Sku,
                    Barcode = variant.Barcode,
                    Weight = variant.Weight,
                    Status = variant.Status,
                    MaxStock = (int)variant.MaxStocks
                };

                return new ResponseDTO<ProductVariantDetailDTO>(
                    data: dto,
                    status: true,
                    message: "Success"
                );
            }
        

    }
}
