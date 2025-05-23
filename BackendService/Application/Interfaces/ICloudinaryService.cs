using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ICloudinaryService
    {
        Task<string> UploadMediaAsync(IFormFile file);
        Task<bool> DeleteMediaAsync(string publicId, string resourceType = "image");
    }
}
