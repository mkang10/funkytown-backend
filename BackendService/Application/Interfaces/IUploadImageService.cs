using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IUploadImageService
    {
        /// <summary>
        /// Upload 1 hình ảnh và trả về đường dẫn URL.
        /// </summary>
        Task<string> UploadImageAsync(IFormFile file);

        /// <summary>
        /// Upload nhiều hình ảnh và trả về danh sách URL.
        /// </summary>
        Task<IEnumerable<string>> UploadImagesAsync(IEnumerable<IFormFile> files);
    }
}
