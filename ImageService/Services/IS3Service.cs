using ImageService.Models.Responses;

namespace ImageService.Services;

public interface IS3Service
{
    Task<UploadUrlResponse> GeneratePresignedUploadUrlAsync(string fileName, string contentType);
    Task<string> UploadImageAsync(Stream imageStream, string fileName, string contentType);
    Task<string> UploadThumbnailAsync(Stream thumbnailStream, string fileName);
    Task<bool> DeleteImageAsync(string imageUrl);
}