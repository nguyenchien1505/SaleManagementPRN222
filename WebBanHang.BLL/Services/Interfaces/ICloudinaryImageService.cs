using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBanHang.BLL.Services.Interfaces
{
    public interface ICloudinaryImageService
    {
        /// <summary>
        /// Upload 1 file ảnh lên Cloudinary, trả về secure URL.
        /// BLL không phụ thuộc IFormFile (kiểu của tầng Web) - chỉ nhận Stream thuần.
        /// </summary>
        /// <param name="fileStream">Stream nội dung file ảnh</param>
        /// <param name="fileName">Tên file gốc (dùng để Cloudinary nhận diện extension)</param>
        /// <param name="folder">Thư mục lưu trên Cloudinary, ví dụ "webbanhang/tickets"</param>
        Task<string?> UploadImageAsync(Stream fileStream, string fileName, string folder);
    }
}
