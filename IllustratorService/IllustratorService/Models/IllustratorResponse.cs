namespace IllustratorService.Models;

public record IllustratorResponse
{
    public int Id { get; init; }
    public string UserId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Bio { get; init; }
    public string? Specialty { get; init; }
    public string? ProfileImageUrl { get; init; }
    public string? WebsiteUrl { get; init; }
    public string? InstagramUrl { get; init; }
    public string? TwitterUrl { get; init; }
    public bool IsAvailableForWork { get; init; }
    public bool IsPublished { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }

    public static IllustratorResponse FromEntity(Illustrator illustrator)
    {
        return new IllustratorResponse
        {
            Id = illustrator.Id,
            UserId = illustrator.UserId,
            Name = illustrator.Name,
            Bio = illustrator.Bio,
            Specialty = illustrator.Specialty,
            ProfileImageUrl = illustrator.ProfileImageUrl,
            WebsiteUrl = illustrator.WebsiteUrl,
            InstagramUrl = illustrator.InstagramUrl,
            TwitterUrl = illustrator.TwitterUrl,
            IsAvailableForWork = illustrator.IsAvailableForWork,
            IsPublished = illustrator.IsPublished,
            CreatedAt = illustrator.CreatedAt,
            UpdatedAt = illustrator.UpdatedAt
        };
    }
}
