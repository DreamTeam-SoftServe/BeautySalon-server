using Amazon.S3;
using Amazon.S3.Model;

namespace API.Services
{
    public interface IImageService
    {
        Task<string> UploadImageAsync(IFormFile file, string folderName);
    }

    public class S3ImageService : IImageService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;

        public S3ImageService(IAmazonS3 s3Client, IConfiguration configuration)
        {
            _s3Client = s3Client;
            _bucketName = configuration["AWS:BucketName"] ?? "beautysalon-dreamteam";
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

            return $"https://{_bucketName}.s3.eu-north-1.amazonaws.com/{uniqueFileName}";
        }
    }
}