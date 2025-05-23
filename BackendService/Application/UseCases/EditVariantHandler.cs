using Application.Interfaces;
using Domain.DTO.Request;
using Domain.DTO.Response;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
    
        public class EditVariantHandler 
        {
            private readonly IProductVarRepos _repository;
            private readonly IUploadImageService _uploadImageService; // injected upload service

            public EditVariantHandler(
                IProductVarRepos repository,
                IUploadImageService uploadImageService)
            {
                _repository = repository;
                _uploadImageService = uploadImageService;
            }

            public async Task<ResponseDTO<EditProductVariantDto>> EditProductVariantAsync(EditProductVariantDto dto)
            {
                var variant = await _repository.GetByIdAsync(dto.VariantId);
                if (variant == null)
                    return new ResponseDTO<EditProductVariantDto>(null!, false, "Variant not found");

                // Map basic fields
                variant.Price = dto.Price;
                variant.Status = dto.Status;
            variant.MaxStocks = dto.MaxStocks;
                // Upload image if present
                if (dto.ImageFile != null)
                {
                    var imageUrl = await _uploadImageService.UploadImageAsync(dto.ImageFile);
                    variant.ImagePath = imageUrl;
                }

                await _repository.UpdateAsync(variant);

                return new ResponseDTO<EditProductVariantDto>(dto, true, "Variant updated successfully");
            }

            public async Task<ResponseDTO<List<ColorDto>>> GetAllColorsByProductAsync()
            {
                var colors = await _repository.GetColorsByProductIdAsync();
                var data = colors.Select(c => new ColorDto
                {
                    ColorId = c.ColorId,
                    ColorName = c.ColorName,
                    ColorCode = c.ColorCode
                }).ToList();

                return new ResponseDTO<List<ColorDto>>(data, true, "Colors retrieved successfully");
            }

            public async Task<ResponseDTO<List<SizeDto>>> GetAllSizesByProductAsync()
            {
                var sizes = await _repository.GetSizesByProductIdAsync();
                var data = sizes.Select(s => new SizeDto
                {
                    SizeId = s.SizeId,
                    SizeName = s.SizeName,
                    SizeDescription = s.SizeDescription
                }).ToList();

                return new ResponseDTO<List<SizeDto>>(data, true, "Sizes retrieved successfully");
            
        }
    }
}
