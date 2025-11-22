using System.ComponentModel.DataAnnotations;

namespace ImageService.Models.DTOs;

public class UpdateImageRequest
{
    [MaxLength(255)]
    public string? Title { get; set; }

    public string? Description { get; set; }

    public List<string>? Tags { get; set; }

    public bool? IsPublished { get; set; }
}