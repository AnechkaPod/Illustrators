using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ImageService.Models.Entities
{
    [Table("images", Schema = "image")]
    public class Image
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [Column("illustrator_id")]
        public Guid IllustratorId { get; set; }

        [Required]
        [MaxLength(255)]
        [Column("title")]
        public string Title { get; set; } = string.Empty;

        [Column("description")]
        public string Description { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        [Column("image_url")]
        public string ImageUrl { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        [Column("thumbnail_url")]
        public string ThumbnailUrl { get; set; } = string.Empty;

        [Column("tags")]
        public List<string> Tags { get; set; } = new();

        [Column("is_published")]
        public bool IsPublished { get; set; } = true;

        [Column("view_count")]
        public int ViewCount { get; set; } = 0;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
