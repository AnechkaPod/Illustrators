using System.ComponentModel.DataAnnotations;

namespace ImageService.Models.DTOs;

public class CreateImageRequest
{
    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    [Required]
    public string ImageUrl { get; set; } = string.Empty;

    [Required]
    public string ThumbnailUrl { get; set; } = string.Empty;

    public List<string> Tags { get; set; } = new();

    public bool IsPublished { get; set; } = true;
}