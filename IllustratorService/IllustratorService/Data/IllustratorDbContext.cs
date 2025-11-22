using IllustratorService.Models;
using Microsoft.EntityFrameworkCore;


namespace IllustratorService.Data;
public class IllustratorDbContext : DbContext
{
    public IllustratorDbContext(DbContextOptions<IllustratorDbContext> options) : base(options)
    {
    }

    public DbSet<Illustrator> Illustrators { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Illustrator>(entity =>
        {
            entity.HasIndex(e => e.UserId)
                .IsUnique()
                .HasDatabaseName("idx_illustrators_user_id");

            entity.HasIndex(e => e.IsPublished)
                .HasDatabaseName("idx_illustrators_is_published");

            entity.HasIndex(e => e.Specialty)
                .HasDatabaseName("idx_illustrators_specialty");

            entity.HasIndex(e => e.IsAvailableForWork)
                .HasDatabaseName("idx_illustrators_is_available");

            entity.HasIndex(e => e.CreatedAt)
                .HasDatabaseName("idx_illustrators_created_at");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
    }
}
