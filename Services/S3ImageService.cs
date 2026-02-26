using Amazon.S3;
using Amazon.S3.Model;

namespace API.Services
{
    public interface IImageService
    {
        Task<string> UploadImageAsync(IFormFile file, string folderName);
        Task DeleteImageAsync(string imageUrl);
    }

    public class S3ImageService : IImageService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;
        private readonly string _baseUrl;

        public S3ImageService(IAmazonS3 s3Client, IConfiguration configuration)
        {
            _s3Client = s3Client;
            _bucketName = configuration["AWS:BucketName"] ?? "beautysalon-dreamteam";
            _baseUrl = $"https://{_bucketName}.s3.eu-north-1.amazonaws.com/";
        }

        public async Task<string> UploadImageAsync(IFormFile file, string folderName)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("Файл порожній");

            var extension = Path.GetExtension(file.FileName);
            var uniqueFileName = $"{folderName}/{Guid.NewGuid()}{extension}";

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);

            var putRequest = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = uniqueFileName,
                InputStream = stream,
                ContentType = file.ContentType
            };

            await _s3Client.PutObjectAsync(putRequest);
            return $"{_baseUrl}{uniqueFileName}";
        }

        public async Task DeleteImageAsync(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl)) return;

            if (!imageUrl.StartsWith(_baseUrl)) return;

            var key = imageUrl.Substring(_baseUrl.Length);

            var deleteRequest = new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = key
            };

            await _s3Client.DeleteObjectAsync(deleteRequest);
        }
    }
}