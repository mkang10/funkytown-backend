using Application.Interfaces;
using AutoMapper;
using Domain.DTO.Request;
using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Application.UseCases.EditProductHandler;

namespace Application.UseCases
{
    
        public class EditProductHandler 
        {
            private readonly IProductRepository _repo;
            private readonly IUploadImageService _uploader;
            private readonly IMapper _mapper;

            public EditProductHandler(
                IProductRepository repo,
                IUploadImageService uploader,
                IMapper mapper)
            {
                _repo = repo;
                _uploader = uploader;
                _mapper = mapper;
            }

        public async Task EditAsync(int id, ProductEditDto dto)
        {
            var product = await _repo.GetByIdAsync(id)
                          ?? throw new KeyNotFoundException($"Product with id={id} not found");

            // 1. Cập nhật thông tin cơ bản
            product.Name = dto.Name;
            product.Description = dto.Description;
            product.CategoryId = dto.CategoryId;
            product.Origin = dto.Origin;
            product.Model = dto.Model;
            product.Occasion = dto.Occasion;
            product.Style = dto.Style;
            product.Material = dto.Material;
            product.Status = dto.Status;

            // 2. Reset tất cả ảnh về IsMain = false để tránh trùng
            foreach (var img in product.ProductImages)
            {
                img.IsMain = false;
            }

            // 3. Cập nhật ảnh tồn tại
            foreach (var upd in dto.ExistingImages)
            {
                var img = product.ProductImages.FirstOrDefault(x => x.ProductImageId == upd.ProductImageId);
                if (img == null) continue;

                if (upd.ImageFile != null)
                {
                    img.ImagePath = await _uploader.UploadImageAsync(upd.ImageFile);
                    img.CreatedDate = DateTime.Now;
                }

                img.IsMain = upd.IsMain;
            }

            // 4. Thêm ảnh mới
            foreach (var add in dto.NewImages)
            {
                if (add.ImageFile == null) continue;

                var url = await _uploader.UploadImageAsync(add.ImageFile);

                product.ProductImages.Add(new ProductImage
                {
                    ImagePath = url,
                    IsMain = add.IsMain,
                    CreatedDate = DateTime.Now,
                    ProductId = product.ProductId
                });
            }

            // 5. Đảm bảo chỉ có duy nhất một ảnh IsMain
            var mainImages = product.ProductImages.Where(x => x.IsMain).OrderByDescending(x => x.CreatedDate).ToList();
            if (!mainImages.Any() && product.ProductImages.Any())
            {
                product.ProductImages.First().IsMain = true;
            }
            else if (mainImages.Count > 1)
            {
                for (int i = 1; i < mainImages.Count; i++)
                {
                    mainImages[i].IsMain = false;
                }
            }

            // 6. Lưu thay đổi
            _repo.Update(product);
            await _repo.SaveChangesAsync();
        }





    }
}
