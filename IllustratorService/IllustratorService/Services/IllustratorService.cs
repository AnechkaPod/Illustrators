using IllustratorService.Data;
using IllustratorService.Models;
using Microsoft.EntityFrameworkCore;

namespace IllustratorService.Services;

public class IllustratorService : IIllustratorService
{
    private readonly IllustratorDbContext _context;
    private readonly ILogger<IllustratorService> _logger;

    public IllustratorService(IllustratorDbContext context, ILogger<IllustratorService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IllustratorResponse> CreateAsync(CreateIllustratorRequest request, string userId)
    {
        var existingProfile = await _context.Illustrators
            .FirstOrDefaultAsync(i => i.UserId == userId);

        if (existingProfile != null)
        {
            _logger.LogWarning("User {UserId} attempted to create duplicate profile", userId);
            throw new InvalidOperationException("An illustrator profile already exists for this user");
        }

        var illustrator = new Illustrator
        {
            UserId = userId,
            Name = request.Name,
            Bio = request.Bio,
            Specialty = request.Specialty,
            ProfileImageUrl = request.ProfileImageUrl,
            WebsiteUrl = request.WebsiteUrl,
            InstagramUrl = request.InstagramUrl,
            TwitterUrl = request.TwitterUrl,
            IsAvailableForWork = request.IsAvailableForWork,
            IsPublished = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _context.Illustrators.AddAsync(illustrator);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created illustrator profile {IllustratorId} for user {UserId}", 
            illustrator.Id, userId);

        return IllustratorResponse.FromEntity(illustrator);
    }

    public async Task<IllustratorResponse?> GetByIdAsync(int id)
    {
        var illustrator = await _context.Illustrators
            .Where(i => i.IsPublished)
            .FirstOrDefaultAsync(i => i.Id == id);

        return illustrator == null ? null : IllustratorResponse.FromEntity(illustrator);
    }

    public async Task<IllustratorResponse?> GetByUserIdAsync(string userId)
    {
        var illustrator = await _context.Illustrators
            .FirstOrDefaultAsync(i => i.UserId == userId);

        return illustrator == null ? null : IllustratorResponse.FromEntity(illustrator);
    }

    public async Task<(IEnumerable<IllustratorResponse> Illustrators, int TotalCount)> GetAllAsync(
        int page = 1,
        int pageSize = 20,
        string? specialty = null,
        bool? isAvailableForWork = null)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 100) pageSize = 100;

        var query = _context.Illustrators
            .Where(i => i.IsPublished)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(specialty))
        {
            query = query.Where(i => i.Specialty != null && i.Specialty.Contains(specialty));
        }

        if (isAvailableForWork.HasValue)
        {
            query = query.Where(i => i.IsAvailableForWork == isAvailableForWork.Value);
        }

        var totalCount = await query.CountAsync();

        var illustrators = await query
            .OrderByDescending(i => i.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var response = illustrators.Select(IllustratorResponse.FromEntity);

        return (response, totalCount);
    }

    public async Task<IllustratorResponse> UpdateAsync(int id, UpdateIllustratorRequest request, string userId)
    {
        var illustrator = await _context.Illustrators.FindAsync(id);

        if (illustrator == null)
        {
            throw new KeyNotFoundException($"Illustrator with ID {id} not found");
        }

        if (illustrator.UserId != userId)
        {
            _logger.LogWarning("User {UserId} attempted to update illustrator {IllustratorId} owned by {OwnerId}", 
                userId, id, illustrator.UserId);
            throw new UnauthorizedAccessException("You can only update your own profile");
        }

        if (request.Name != null) illustrator.Name = request.Name;
        if (request.Bio != null) illustrator.Bio = request.Bio;
        if (request.Specialty != null) illustrator.Specialty = request.Specialty;
        if (request.ProfileImageUrl != null) illustrator.ProfileImageUrl = request.ProfileImageUrl;
        if (request.WebsiteUrl != null) illustrator.WebsiteUrl = request.WebsiteUrl;
        if (request.InstagramUrl != null) illustrator.InstagramUrl = request.InstagramUrl;
        if (request.TwitterUrl != null) illustrator.TwitterUrl = request.TwitterUrl;
        if (request.IsAvailableForWork.HasValue) illustrator.IsAvailableForWork = request.IsAvailableForWork.Value;
        if (request.IsPublished.HasValue) illustrator.IsPublished = request.IsPublished.Value;

        illustrator.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated illustrator profile {IllustratorId}", id);

        return IllustratorResponse.FromEntity(illustrator);
    }

    public async Task DeleteAsync(int id, string userId)
    {
        var illustrator = await _context.Illustrators.FindAsync(id);

        if (illustrator == null)
        {
            throw new KeyNotFoundException($"Illustrator with ID {id} not found");
        }

        if (illustrator.UserId != userId)
        {
            _logger.LogWarning("User {UserId} attempted to delete illustrator {IllustratorId} owned by {OwnerId}", 
                userId, id, illustrator.UserId);
            throw new UnauthorizedAccessException("You can only delete your own profile");
        }

        _context.Illustrators.Remove(illustrator);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted illustrator profile {IllustratorId}", id);
    }
}
