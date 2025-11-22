using Microsoft.EntityFrameworkCore;
using ImageService.Models.Entities;

namespace ImageService.Data;

public class ImageDbContext : DbContext
{
    public ImageDbContext(DbContextOptions<ImageDbContext> options) : base(options)
    {
    }

    public DbSet<Image> Images { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("image");

        modelBuilder.Entity<Image>(entity =>
        {
            entity.ToTable("images", "image");
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.IllustratorId);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.IsPublished);

            entity.Property(e => e.Id)
                .IsRequired()
                .ValueGeneratedOnAdd();

            entity.Property(e => e.IllustratorId)
                .IsRequired();

            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.Description)
                .HasDefaultValue(string.Empty);

            entity.Property(e => e.ImageUrl)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.ThumbnailUrl)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.Tags)
                .HasColumnType("text[]");

            entity.Property(e => e.IsPublished)
                .IsRequired()
                .HasDefaultValue(true);

            entity.Property(e => e.ViewCount)
                .IsRequired()
                .HasDefaultValue(0);

            entity.Property(e => e.CreatedAt)
                .IsRequired();

            entity.Property(e => e.UpdatedAt)
                .IsRequired();
        });
    }
}