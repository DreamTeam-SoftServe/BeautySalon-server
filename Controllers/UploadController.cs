using API.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UploadController : ControllerBase
    {
        private readonly IImageService _imageService;

        public UploadController(IImageService imageService)
        {
            _imageService = imageService;
        }

        [HttpPost("image")]
        public async Task<IActionResult> UploadImage(IFormFile file, [FromQuery] string folder = "general")
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest("No file selected.");

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                var extension = Path.GetExtension(file.FileName).ToLower();

                if (!allowedExtensions.Contains(extension))
                    return BadRequest("Only JPG, PNG, WEBP are allowed.");

                if (file.Length > 5 * 1024 * 1024)
                    return BadRequest("The file size must not exceed 5 MB..");

                var result = await _imageService.UploadImageAsync(file, folder);
                return Ok(new { Url = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Download error: {ex.Message}");
            }
        }
    }
}