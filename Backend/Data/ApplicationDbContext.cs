using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProjetFullstack.Models;

namespace ProjetFullstack.Data;

public class ApplicationDbContext
    : IdentityDbContext<
        ApplicationUser,
        IdentityRole<Guid>,
        Guid>
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<RefreshToken> RefreshTokens =>
        Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ============================================================
        // APPLICATION USER
        // ============================================================

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(u => u.FirstName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(u => u.LastName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(u => u.CreatedAt)
                .IsRequired();

            entity.Property(u => u.UpdatedAt)
                .IsRequired();

            entity.Property(u => u.IsActive)
                .IsRequired();
        });

        // ============================================================
        // REFRESH TOKEN
        // ============================================================

        builder.Entity<RefreshToken>(entity =>
        {
            // Primary Key
            entity.HasKey(x => x.Id);

            // Token hash
            entity.Property(x => x.TokenHash)
                .IsRequired()
                .HasMaxLength(128);

            // Foreign Key vers ApplicationUser.Id
            entity.Property(x => x.UserId)
                .IsRequired();

            // Un refresh token hash doit être unique
            entity.HasIndex(x => x.TokenHash)
                .IsUnique();

            // Relation :
            // ApplicationUser 1 ---- 0..* RefreshToken
            entity.HasOne(x => x.User)
                .WithMany(x => x.RefreshTokens)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
