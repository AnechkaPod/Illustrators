using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IllustratorService.Models;

[Table("illustrators")]
public class Illustrator
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("user_id")]
    [MaxLength(450)]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [Column("name")]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Column("bio")]
    [MaxLength(2000)]
    public string? Bio { get; set; }

    [Column("specialty")]
    [MaxLength(200)]
    public string? Specialty { get; set; }

    [Column("profile_image_url")]
    [MaxLength(1000)]
    public string? ProfileImageUrl { get; set; }

    [Column("website_url")]
    [MaxLength(500)]
    public string? WebsiteUrl { get; set; }

    [Column("instagram_url")]
    [MaxLength(500)]
    public string? InstagramUrl { get; set; }

    [Column("twitter_url")]
    [MaxLength(500)]
    public string? TwitterUrl { get; set; }

    [Required]
    [Column("is_available_for_work")]
    public bool IsAvailableForWork { get; set; } = true;

    [Required]
    [Column("is_published")]
    public bool IsPublished { get; set; } = true;

    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
