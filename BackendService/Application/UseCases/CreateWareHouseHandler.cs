using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using Domain.DTO.Request;
using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class CreateWarehouseHandler
    {
        private readonly IWarehouseRepository _warehouseRepo;
        private readonly Cloudinary _cloudinary;

        public CreateWarehouseHandler(IWarehouseRepository warehouseRepo, Cloudinary cloudinary)
        {
            _warehouseRepo = warehouseRepo;
            _cloudinary = cloudinary;
        }

        public async Task<int> HandleAsync(WarehouseCreateDto dto)
        {
            string? imagePath = null;
            if (dto.ImageFile != null)
            {
                using var stream = dto.ImageFile.OpenReadStream();
                var uploadParams = new ImageUploadParams { File = new FileDescription(dto.ImageFile.FileName, stream) };
                var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                imagePath = uploadResult.SecureUrl.AbsoluteUri;
            }

            var warehouse = new Warehouse
            {
                WarehouseName = dto.WarehouseName,
                WarehouseDescription = dto.WarehouseDescription,
                Location = dto.Location,
                CreatedDate = DateTime.Now,
                ImagePath = imagePath,
                Email = dto.Email,
                Phone = dto.Phone,
                WarehouseType = dto.WarehouseType,
                ShopManagerId = dto.ShopManagerId,
                IsOwnerWarehouse = dto.IsOwnerWarehouse
            };

            var created = await _warehouseRepo.CreateAsync(warehouse);
            return created.WarehouseId;
        }
    }
}