using Microsoft.EntityFrameworkCore;
using ImageService.Data;
using ImageService.Models.DTOs;
using ImageService.Models.Entities;

namespace ImageService.Services;

public class ImageManagementService : IImageService
{
    private readonly ImageDbContext _context;
    private readonly ILogger<ImageManagementService> _logger;

    public ImageManagementService(ImageDbContext context, ILogger<ImageManagementService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PagedResponse<ImageResponse>> GetAllImagesAsync(
        int page,
        int pageSize,
        string? tags,
        string sortBy)
    {
        var query = _context.Images.Where(i => i.IsPublished);

        // Filter by tags if provided
        if (!string.IsNullOrEmpty(tags))
        {
            var tagList = tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim().ToLower())
                .ToList();

            query = query.Where(i => i.Tags.Any(t => tagList.Contains(t.ToLower())));
        }

        // Sort
        query = sortBy.ToLower() switch
        {
            "recent" => query.OrderByDescending(i => i.CreatedAt),
            "popular" => query.OrderByDescending(i => i.ViewCount),
            "title" => query.OrderBy(i => i.Title),
            _ => query.OrderByDescending(i => i.CreatedAt)
        };

        var totalCount = await query.CountAsync();

        var images = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(i => new ImageResponse
            {
                Id = i.Id,
                IllustratorId = i.IllustratorId,
                Title = i.Title,
                Description = i.Description,
                ImageUrl = i.ImageUrl,
                ThumbnailUrl = i.ThumbnailUrl,
                Tags = i.Tags,
                IsPublished = i.IsPublished,
                ViewCount = i.ViewCount,
                CreatedAt = i.CreatedAt,
                UpdatedAt = i.UpdatedAt
            })
            .ToListAsync();

        return new PagedResponse<ImageResponse>
        {
            Items = images,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    public async Task<ImageResponse?> GetImageByIdAsync(Guid id)
    {
        var image = await _context.Images.FindAsync(id);

        if (image == null)
            return null;

        return new ImageResponse
        {
            Id = image.Id,
            IllustratorId = image.IllustratorId,
            Title = image.Title,
            Description = image.Description,
            ImageUrl = image.ImageUrl,
            ThumbnailUrl = image.ThumbnailUrl,
            Tags = image.Tags,
            IsPublished = image.IsPublished,
            ViewCount = image.ViewCount,
            CreatedAt = image.CreatedAt,
            UpdatedAt = image.UpdatedAt
        };
    }

    public async Task<PagedResponse<ImageResponse>> GetImagesByIllustratorAsync(
        Guid illustratorId,
        int page,
        int pageSize)
    {
        var query = _context.Images
            .Where(i => i.IllustratorId == illustratorId && i.IsPublished)
            .OrderByDescending(i => i.CreatedAt);

        var totalCount = await query.CountAsync();

        var images = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(i => new ImageResponse
            {
                Id = i.Id,
                IllustratorId = i.IllustratorId,
                Title = i.Title,
                Description = i.Description,
                ImageUrl = i.ImageUrl,
                ThumbnailUrl = i.ThumbnailUrl,
                Tags = i.Tags,
                IsPublished = i.IsPublished,
                ViewCount = i.ViewCount,
                CreatedAt = i.CreatedAt,
                UpdatedAt = i.UpdatedAt
            })
            .ToListAsync();

        return new PagedResponse<ImageResponse>
        {
            Items = images,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    public async Task<ImageResponse> CreateImageAsync(
        Guid illustratorId,
        CreateImageRequest request)
    {
        var image = new Image
        {
            Id = Guid.NewGuid(),
            IllustratorId = illustratorId,
            Title = request.Title,
            Description = request.Description,
            ImageUrl = request.ImageUrl,
            ThumbnailUrl = request.ThumbnailUrl,
            Tags = request.Tags,
            IsPublished = request.IsPublished,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Images.Add(image);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Image created: {ImageId} for illustrator {IllustratorId}",
            image.Id, illustratorId);

        return new ImageResponse
        {
            Id = image.Id,
            IllustratorId = image.IllustratorId,
            Title = image.Title,
            Description = image.Description,
            ImageUrl = image.ImageUrl,
            ThumbnailUrl = image.ThumbnailUrl,
            Tags = image.Tags,
            IsPublished = image.IsPublished,
            ViewCount = image.ViewCount,
            CreatedAt = image.CreatedAt,
            UpdatedAt = image.UpdatedAt
        };
    }

    public async Task<ImageResponse?> UpdateImageAsync(
        Guid id,
        Guid userId,
        UpdateImageRequest request)
    {
        var image = await _context.Images.FindAsync(id);

        if (image == null)
            return null;

        // Verify ownership
        if (image.IllustratorId != userId)
        {
            _logger.LogWarning("User {UserId} attempted to update image {ImageId} owned by {OwnerId}",
                userId, id, image.IllustratorId);
            return null;
        }

        // Update fields if provided
        if (request.Title != null)
            image.Title = request.Title;

        if (request.Description != null)
            image.Description = request.Description;

        if (request.Tags != null)
            image.Tags = request.Tags;

        if (request.IsPublished.HasValue)
            image.IsPublished = request.IsPublished.Value;

        image.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Image updated: {ImageId}", id);

        return new ImageResponse
        {
            Id = image.Id,
            IllustratorId = image.IllustratorId,
            Title = image.Title,
            Description = image.Description,
            ImageUrl = image.ImageUrl,
            ThumbnailUrl = image.ThumbnailUrl,
            Tags = image.Tags,
            IsPublished = image.IsPublished,
            ViewCount = image.ViewCount,
            CreatedAt = image.CreatedAt,
            UpdatedAt = image.UpdatedAt
        };
    }

    public async Task<bool> DeleteImageAsync(Guid id, Guid userId)
    {
        var image = await _context.Images.FindAsync(id);

        if (image == null)
            return false;

        // Verify ownership
        if (image.IllustratorId != userId)
        {
            _logger.LogWarning("User {UserId} attempted to delete image {ImageId} owned by {OwnerId}",
                userId, id, image.IllustratorId);
            return false;
        }

        _context.Images.Remove(image);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Image deleted: {ImageId}", id);

        return true;
    }

    public async Task IncrementViewCountAsync(Guid id)
    {
        var image = await _context.Images.FindAsync(id);

        if (image != null)
        {
            image.ViewCount++;
            await _context.SaveChangesAsync();
        }
    }
}