using Amazon.S3;
using Amazon.S3.Model;
using ImageService.Models.Responses;

namespace ImageService.Services;

public class S3Service : IS3Service
{
    private readonly IAmazonS3 _s3Client;
    private readonly IConfiguration _configuration;
    private readonly ILogger<S3Service> _logger;
    private readonly string _bucketName;

    public S3Service(
        IAmazonS3 s3Client,
        IConfiguration configuration,
        ILogger<S3Service> logger)
    {
        _s3Client = s3Client;
        _configuration = configuration;
        _logger = logger;
        _bucketName = _configuration["AWS:S3:BucketName"]
            ?? throw new InvalidOperationException("S3 bucket name not configured");
    }

    public async Task<UploadUrlResponse> GeneratePresignedUploadUrlAsync(
        string fileName,
        string contentType)
    {
        try
        {
            var key = $"images/{Guid.NewGuid()}/{fileName}";
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _bucketName,
                Key = key,
                Verb = HttpVerb.PUT,
                Expires = DateTime.UtcNow.AddMinutes(15),
                ContentType = contentType
            };

            var uploadUrl = _s3Client.GetPreSignedURL(request);
            var imageUrl = $"https://{_bucketName}.s3.amazonaws.com/{key}";

            return new UploadUrlResponse
            {
                UploadUrl = uploadUrl,
                ImageKey = key,
                FinalImageUrl = imageUrl,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating presigned URL");
            throw;
        }
    }

    public async Task<string> UploadImageAsync(
        Stream imageStream,
        string fileName,
        string contentType)
    {
        try
        {
            var key = $"images/{Guid.NewGuid()}/{fileName}";

            var request = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = key,
                InputStream = imageStream,
                ContentType = contentType,
                CannedACL = S3CannedACL.PublicRead
            };

            await _s3Client.PutObjectAsync(request);

            return $"https://{_bucketName}.s3.amazonaws.com/{key}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading image to S3");
            throw;
        }
    }

    public async Task<string> UploadThumbnailAsync(Stream thumbnailStream, string fileName)
    {
        try
        {
            var key = $"thumbnails/{Guid.NewGuid()}/{fileName}";

            var request = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = key,
                InputStream = thumbnailStream,
                ContentType = "image/jpeg",
                CannedACL = S3CannedACL.PublicRead
            };

            await _s3Client.PutObjectAsync(request);

            return $"https://{_bucketName}.s3.amazonaws.com/{key}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading thumbnail to S3");
            throw;
        }
    }

    public async Task<bool> DeleteImageAsync(string imageUrl)
    {
        try
        {
            // Extract key from URL
            var uri = new Uri(imageUrl);
            var key = uri.AbsolutePath.TrimStart('/');

            var request = new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = key
            };

            await _s3Client.DeleteObjectAsync(request);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting image from S3");
            return false;
        }
    }
}