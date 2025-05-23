using Application.Interfaces;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.HelperServices
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IOptions<CloudinarySettings> cloudinarySettings)
        {
            var settings = cloudinarySettings.Value;
            if (string.IsNullOrEmpty(settings.CloudName) ||
                string.IsNullOrEmpty(settings.ApiKey) ||
                string.IsNullOrEmpty(settings.ApiSecret))
            {
                throw new ArgumentException("Cloudinary configuration is missing or invalid.");
            }

            var account = new CloudinaryDotNet.Account(settings.CloudName, settings.ApiKey, settings.ApiSecret);
            _cloudinary = new Cloudinary(account);
        }

        public async Task<string> UploadMediaAsync(IFormFile file)
        {
            if (file == null || file.Length == 0) return string.Empty;

            using var stream = file.OpenReadStream();
            var fileExtension = Path.GetExtension(file.FileName).ToLower();

            UploadResult uploadResult;

            if (fileExtension == ".jpg" || fileExtension == ".jpeg" || fileExtension == ".png" || fileExtension == ".gif")
            {
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = "Images"
                };
                uploadResult = await _cloudinary.UploadAsync(uploadParams);
            }
            else if (fileExtension == ".mp4" || fileExtension == ".mov" || fileExtension == ".avi")
            {
                var uploadParams = new VideoUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = "Images"
                };
                uploadResult = await _cloudinary.UploadAsync(uploadParams);
            }
            else
            {
                var uploadParams = new RawUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = "Images"
                };
                uploadResult = await _cloudinary.UploadAsync(uploadParams);
            }

            return uploadResult.SecureUrl?.ToString() ?? string.Empty;
        }

        public async Task<bool> DeleteMediaAsync(string publicId, string resourceType = "image")
        {
            if (!Enum.TryParse(resourceType, true, out ResourceType cloudinaryResourceType))
            {
                cloudinaryResourceType = ResourceType.Image;
            }

            var deleteParams = new DeletionParams(publicId)
            {
                ResourceType = cloudinaryResourceType
            };

            var result = await _cloudinary.DestroyAsync(deleteParams);
            return result.Result == "ok";
        }
    }
    public class CloudinarySettings
    {
        public string CloudName { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string ApiSecret { get; set; } = string.Empty;
    }
}
