// Services/LocalFileService.cs
using ImageService.Models.Responses;
using ImageService.Services;

public class LocalFileService : IS3Service
{
    private readonly string _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");

    public LocalFileService()
    {
        Directory.CreateDirectory(_uploadPath);
        Directory.CreateDirectory(Path.Combine(_uploadPath, "images"));
        Directory.CreateDirectory(Path.Combine(_uploadPath, "thumbnails"));
    }

    public Task<string> UploadImageAsync(Stream imageStream, string fileName, string contentType)
    {
        var filePath = Path.Combine(_uploadPath, "images", fileName);
        using var fileStream = File.Create(filePath);
        imageStream.CopyTo(fileStream);
        return Task.FromResult($"http://localhost:5003/uploads/images/{fileName}");
    }

    public Task<string> UploadThumbnailAsync(Stream thumbnailStream, string fileName)
    {
        var filePath = Path.Combine(_uploadPath, "thumbnails", fileName);
        using var fileStream = File.Create(filePath);
        thumbnailStream.CopyTo(fileStream);
        return Task.FromResult($"http://localhost:5003/uploads/thumbnails/{fileName}");
    }

    public Task<bool> DeleteImageAsync(string imageUrl)
    {
        // Extract filename from URL and delete
        return Task.FromResult(true);
    }

    public Task<UploadUrlResponse> GeneratePresignedUploadUrlAsync(string fileName, string contentType)
    {
        return Task.FromResult(new UploadUrlResponse
        {
            UploadUrl = $"http://localhost:5003/api/images/upload",
            ImageKey = fileName,
            FinalImageUrl = $"http://localhost:5003/uploads/images/{fileName}",
            ExpiresAt = DateTime.UtcNow.AddMinutes(15)
        });
    }
}