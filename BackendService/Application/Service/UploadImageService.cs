using Application.Interfaces;
using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Service
{
    public class UploadImageService : IUploadImageService
    {
        private readonly Cloudinary _cloudinary;

        public UploadImageService(Cloudinary cloudinary)
        {
            _cloudinary = cloudinary;
        }

        public async Task<string> UploadImageAsync(IFormFile file)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream)
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception("Error during image upload.");
            }

            return uploadResult.SecureUrl.AbsoluteUri;
        }

        public async Task<IEnumerable<string>> UploadImagesAsync(IEnumerable<IFormFile> files)
        {
            var imageUrls = new List<string>();

            foreach (var file in files)
            {
                string url = await UploadImageAsync(file);
                imageUrls.Add(url);
            }

            return imageUrls;
        }
    }
}
