using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadImageController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;

        public UploadImageController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        [HttpPost("image")]
        public async Task<IActionResult> UploadImage([FromForm] UploadImageDto model)
        {
            // Kiểm tra trường hợp nhập URL
            if (!string.IsNullOrEmpty(model.ImageUrl))
            {
                // Validate URL
                if (!Uri.TryCreate(model.ImageUrl, UriKind.Absolute, out var uriResult) ||
                    !(uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
                {
                    return BadRequest(new { message = "Invalid image URL." });
                }

                // Trả về URL nếu hợp lệ
                return Ok(new
                {
                    message = "Image URL received successfully!",
                    url = model.ImageUrl
                });
            }

            // Kiểm tra trường hợp upload file
            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                // Validate file type
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var extension = Path.GetExtension(model.ImageFile.FileName)?.ToLower();
                if (string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension))
                {
                    return BadRequest(new { message = "Only image files are allowed (jpg, jpeg, png, gif)." });
                }

                try
                {
                    // Đảm bảo thư mục upload tồn tại
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Tạo tên file duy nhất
                    var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Lưu file
                    await using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ImageFile.CopyToAsync(stream);
                    }

                    // Tạo URL trả về
                    var fileUrl = $"{Request.Scheme}://{Request.Host}/uploads/{uniqueFileName}";
                    return Ok(new
                    {
                        message = "Image file uploaded successfully!",
                        url = fileUrl
                    });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { message = "Internal server error occurred.", error = ex.Message });
                }
            }

            // Nếu không có file hoặc URL
            return BadRequest(new { message = "No file uploaded or image URL provided." });
        }
    }

    public class UploadImageDto
    {
        public IFormFile ImageFile { get; set; } // Dành cho upload file
        public string ImageUrl { get; set; } // Dành cho URL ảnh
    }
}
