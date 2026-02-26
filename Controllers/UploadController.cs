using API.Services;
using Microsoft.AspNetCore.Authorization;
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

        private static readonly Dictionary<string, byte[][]> _magicBytes = new()
        {
            { ".jpg",  new[] { new byte[] { 0xFF, 0xD8, 0xFF } } },
            { ".jpeg", new[] { new byte[] { 0xFF, 0xD8, 0xFF } } },
            { ".png",  new[] { new byte[] { 0x89, 0x50, 0x4E, 0x47 } } },
            { ".webp", new[] { new byte[] { 0x52, 0x49, 0x46, 0x46 } } },
        };

        private static async Task<bool> IsValidImageAsync(IFormFile file, string extension)
        {
            if (!_magicBytes.TryGetValue(extension, out var signatures))
                return false;

            var buffer = new byte[8];
            using var stream = file.OpenReadStream();
            var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

            return signatures.Any(sig =>
                bytesRead >= sig.Length &&
                sig.SequenceEqual(buffer.Take(sig.Length))
            );
        }

        [Authorize]
        [HttpPost("image")]
        public async Task<IActionResult> UploadImage(
            IFormFile file,
            [FromQuery] string folder = "general")
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { message = "No file selected." });

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                var extension = Path.GetExtension(file.FileName).ToLower();
                if (!allowedExtensions.Contains(extension))
                    return BadRequest(new { message = "Only JPG, PNG, WEBP are allowed." });

                if (file.Length > 5 * 1024 * 1024)
                    return BadRequest(new { message = "File size must not exceed 5 MB." });

                if (!await IsValidImageAsync(file, extension))
                    return BadRequest(new { message = "File content does not match its extension." });

                var allowedFolders = new[] { "general", "masters", "services", "portfolio", "hero" };
                if (!allowedFolders.Contains(folder.ToLower()))
                    return BadRequest(new { message = "Invalid folder." });

                var url = await _imageService.UploadImageAsync(file, folder.ToLower());
                return Ok(new { url });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Upload error: {ex.Message}" });
            }
        }
    }
}