using Application.Interfaces; // Interface của UploadImageService
using Domain.DTO.Request;
using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class CreateProductHandler
    {
        private readonly IProductRepository _productRepo;
        private readonly IProductVarRepos _variantRepo;
        private readonly IUploadImageService _uploadImageService;
        private readonly IImportRepos _importDetailRepo;
        private readonly IWareHousesStockRepository _stockRepo;


        public CreateProductHandler(
            IProductRepository productRepo,
            IProductVarRepos variantRepo,
            IUploadImageService uploadImageService,
            IImportRepos importDetailRepos,
            IWareHousesStockRepository stockRepo)
        {
            _productRepo = productRepo;
            _variantRepo = variantRepo;
            _uploadImageService = uploadImageService;
            _importDetailRepo = importDetailRepos;
            _stockRepo = stockRepo;
        }

        public async Task<int> CreateProductAsync(ProductCreateDto dto)
        {
            // Upload images sử dụng UploadImageService đã tách riêng
            var imageDtos = new List<ProductImageDto>();
            var imageUrls = await _uploadImageService.UploadImagesAsync(dto.Images);

            int count = 0;
            foreach (var url in imageUrls)
            {
                imageDtos.Add(new ProductImageDto
                {
                    ImagePath = url,
                    IsMain = count == 0   // Ảnh đầu tiên được đặt làm ảnh chính
                });
                count++;
            }

            // Map dữ liệu từ DTO sang entity Product
            var product = new Product
            {
                Name = dto.Name,
                Description = dto.Description,
                CategoryId = dto.CategoryId,
                Origin = dto.Origin,
                Model = dto.Model,
                Occasion = dto.Occasion,
                Style = dto.Style,
                Material = dto.Material,
                Status = dto.Status,
                ProductImages = imageDtos.Select(img => new ProductImage
                {
                    ImagePath = img.ImagePath,
                    IsMain = img.IsMain,
                    CreatedDate = DateTime.Now
                }).ToList()
            };

            var created = await _productRepo.CreateAsync(product);
            return created.ProductId;
        }
        private string GenerateSku(int productId, int sizeId, int colorId)
        {
            // SKU format: SKU-P[productId]-S[sizeId]-C[colorId]-[random]
            var randomSuffix = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
            return $"SKU-P{productId}-S{sizeId}-C{colorId}-{randomSuffix}";
        }

        private string GenerateBar(int productId, int sizeId, int colorId)
        {
            // SKU định dạng: PROD-[productId]-S[sizeId]-C[colorId]-[random]
            var randomSuffix = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
            return $"BAR-{productId}-S{sizeId}-C{colorId}-{randomSuffix}";
        }
        public async Task<int> CreateVariantAsync(ProductVariantCreateDto dto)
        {
            var sku = GenerateSku(dto.ProductId, (int)dto.SizeId, (int)dto.ColorId);

            var bar = GenerateBar(dto.ProductId, (int)dto.SizeId, (int)dto.ColorId);

            var product = await _productRepo.GetByIdAsync(dto.ProductId);
            if (product == null)
                throw new InvalidOperationException("Product không tồn tại");

            // 3. Kiểm tra trùng biến thể (ProductId + SizeId + ColorId)
            var existingVariant = await _variantRepo.GetByProductSizeColorAsync(dto.ProductId, (int)dto.SizeId, (int)dto.ColorId);
            if (existingVariant != null)
                throw new InvalidOperationException("Biến thể với cùng Size và Color đã tồn tại cho sản phẩm này");

            // 4. Upload hình ảnh (nếu có)
            string? imagePath = null;
            if (dto.ImageFile != null)
                imagePath = await _uploadImageService.UploadImageAsync(dto.ImageFile);

            // 5. Tạo variant mới
            var variant = new ProductVariant
            {
                MaxStocks = dto.MaxStocks,
                ProductId = dto.ProductId,
                SizeId = dto.SizeId,
                ColorId = dto.ColorId,
                Price = 0,
                ImagePath = imagePath,
                Sku = sku,
                Barcode = bar,
                Weight = dto.Weight,
                Status = "Draft"
                
            };

            var created = await _variantRepo.CreateAsync(variant);
            return created.VariantId;
        }
    }
}

