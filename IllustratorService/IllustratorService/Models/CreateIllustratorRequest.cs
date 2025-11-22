using System.ComponentModel.DataAnnotations;
namespace IllustratorService.Models;

public record CreateIllustratorRequest
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 200 characters")]
    public string Name { get; init; } = string.Empty;

    [StringLength(2000, ErrorMessage = "Bio cannot exceed 2000 characters")]
    public string? Bio { get; init; }

    [StringLength(200, ErrorMessage = "Specialty cannot exceed 200 characters")]
    public string? Specialty { get; init; }

    [Url(ErrorMessage = "Profile image URL must be a valid URL")]
    [StringLength(1000, ErrorMessage = "Profile image URL cannot exceed 1000 characters")]
    public string? ProfileImageUrl { get; init; }

    [Url(ErrorMessage = "Website URL must be a valid URL")]
    [StringLength(500, ErrorMessage = "Website URL cannot exceed 500 characters")]
    public string? WebsiteUrl { get; init; }

    [Url(ErrorMessage = "Instagram URL must be a valid URL")]
    [StringLength(500, ErrorMessage = "Instagram URL cannot exceed 500 characters")]
    public string? InstagramUrl { get; init; }

    [Url(ErrorMessage = "Twitter URL must be a valid URL")]
    [StringLength(500, ErrorMessage = "Twitter URL cannot exceed 500 characters")]
    public string? TwitterUrl { get; init; }

    public bool IsAvailableForWork { get; init; } = true;
}
