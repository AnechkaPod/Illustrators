using ImageService.Models.DTOs;

namespace ImageService.Services;

public interface IImageService
{
    Task<PagedResponse<ImageResponse>> GetAllImagesAsync(int page, int pageSize, string? tags, string sortBy);
    Task<ImageResponse?> GetImageByIdAsync(Guid id);
    Task<PagedResponse<ImageResponse>> GetImagesByIllustratorAsync(Guid illustratorId, int page, int pageSize);
    Task<ImageResponse> CreateImageAsync(Guid illustratorId, CreateImageRequest request);
    Task<ImageResponse?> UpdateImageAsync(Guid id, Guid userId, UpdateImageRequest request);
    Task<bool> DeleteImageAsync(Guid id, Guid userId);
    Task IncrementViewCountAsync(Guid id);
}