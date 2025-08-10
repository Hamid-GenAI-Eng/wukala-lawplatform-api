using Microsoft.EntityFrameworkCore;
using UserAuthAPI.Data.Entities;

namespace UserAuthAPI.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Document> Documents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User entity configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.HasIndex(e => e.Email)
                  .IsUnique()
                  .HasDatabaseName("IX_Users_Email");

            entity.HasIndex(e => e.GoogleId)
                  .IsUnique()
                  .HasDatabaseName("IX_Users_GoogleId")
                  .HasFilter("GoogleId IS NOT NULL");

            entity.Property(e => e.Email)
                  .IsRequired()
                  .HasMaxLength(255);

            entity.Property(e => e.Name)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(e => e.PasswordHash)
                  .HasMaxLength(255);

            entity.Property(e => e.Provider)
                  .IsRequired()
                  .HasMaxLength(50)
                  .HasDefaultValue("Local");

            entity.Property(e => e.GoogleId)
                  .HasMaxLength(255);

            entity.Property(e => e.CreatedAt)
                  .IsRequired()
                  .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.UpdatedAt);
        });

        // Document entity configuration
        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.Property(d => d.FileName)
                  .IsRequired()
                  .HasMaxLength(255);
            entity.Property(d => d.FileType)
                  .IsRequired()
                  .HasMaxLength(50);
            entity.Property(d => d.UploadDate)
                  .IsRequired()
                  .HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(d => d.EncryptedContent)
                  .IsRequired();

            entity.HasOne(d => d.User)
                  .WithMany()
                  .HasForeignKey(d => d.UserId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("FK_Documents_Users_UserId");

            entity.HasIndex(d => new { d.UserId, d.UploadDate })
                  .HasDatabaseName("IX_Documents_UserId_UploadDate");
        });
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries<User>();

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }
    }
}