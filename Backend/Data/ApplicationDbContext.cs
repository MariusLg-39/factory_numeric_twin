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

            // Nullable dans le modèle
            entity.Property(u => u.UpdatedAt)
                .IsRequired(false);

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

        // ============================================================
        // PRODUCTION LINE
        // ============================================================

        builder.Entity<ProductionLine>(entity =>
        {
            // Primary Key
            entity.HasKey(x => x.Id);

            // Name
            entity.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);

            // Description
            entity.Property(x => x.Description)
                .IsRequired()
                .HasMaxLength(500);

            // Status
            entity.Property(x => x.Status)
                .IsRequired()
                .HasMaxLength(50);

            // CreatedAt
            entity.Property(x => x.CreatedAt)
                .IsRequired();

            // UpdatedAt
            entity.Property(x => x.UpdatedAt)
                .IsRequired();
        });

        // ============================================================
        // PRODUCT
        // ============================================================

        builder.Entity<Product>(entity =>
        {
            // Primary Key
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);

            // Foreign Key vers ProductionLine.Id
            entity.Property(x => x.ProductionLineId)
                .IsRequired();

            // Relation :
            // ProductionLine 1 ---- 0..* Product
            entity.HasOne(p => p.ProductionLine)
                .WithMany(pl => pl.Products)
                .HasForeignKey(p => p.ProductionLineId);
        });

        // ============================================================
        // MACHINE
        // ============================================================

        builder.Entity<Machine>(entity =>
        {
            // Primary Key
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(x => x.Status)
                .IsRequired()
                .HasMaxLength(50);

            // Placement dans le jumeau 3D
            entity.Property(x => x.PositionX).IsRequired();
            entity.Property(x => x.PositionY).IsRequired();
            entity.Property(x => x.PositionZ).IsRequired();
            entity.Property(x => x.RotationY).IsRequired();
            entity.Property(x => x.Scale).IsRequired();

            // Relation :
            // MachineModel 1 ---- 0..* Machine (suppression du modèle interdite s'il est utilisé)
            entity.HasOne(m => m.MachineModel)
                .WithMany(mm => mm.Machines)
                .HasForeignKey(m => m.MachineModelId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(x => x.CreatedAt)
                .IsRequired();

            entity.Property(x => x.UpdatedAt)
                .IsRequired();

            // Foreign Key vers ProductionLine.Id
            entity.Property(x => x.ProductionLineId)
                .IsRequired();

            // Relation :
            // ProductionLine 1 ---- 0..* Machine
            entity.HasOne(m => m.ProductionLine)
                .WithMany(pl => pl.Machines)
                .HasForeignKey(m => m.ProductionLineId);
        });

        // ============================================================
        // MACHINE MODEL (catalogue)
        // ============================================================

        builder.Entity<MachineModel>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(x => x.Type)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(x => x.Description)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(x => x.Model3DPath)
                .HasMaxLength(255);

            entity.Property(x => x.Model3DFileName)
                .HasMaxLength(255);

            entity.Property(x => x.CreatedAt)
                .IsRequired();

            entity.Property(x => x.UpdatedAt)
                .IsRequired();
        });

        // ============================================================
        // MACHINE MODEL CONFIGURATION (configurations par défaut)
        // ============================================================

        builder.Entity<MachineModelConfiguration>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(x => x.Unit)
                .IsRequired()
                .HasMaxLength(50);

            // Relation :
            // MachineModel 1 ---- 0..* MachineModelConfiguration
            entity.HasOne(c => c.MachineModel)
                .WithMany(m => m.DefaultConfigurations)
                .HasForeignKey(c => c.MachineModelId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ============================================================
        // MACHINE LOG
        // ============================================================

        builder.Entity<MachineLog>(entity =>
        {
            // Primary Key
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Message)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(x => x.Type)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(x => x.CreatedAt)
                .IsRequired();

            // Foreign Key vers Machine.Id
            entity.Property(x => x.MachineId)
                .IsRequired();

            // Relation :
            // Machine 1 ---- 0..* MachineLog
            entity.HasOne(ml => ml.Machine)
                .WithMany(m => m.MachineLogs)
                .HasForeignKey(ml => ml.MachineId);
        });

        // ============================================================
        // MACHINE CONFIGURATION
        // ============================================================

        builder.Entity<MachineConfiguration>(entity =>
        {
            // Primary Key
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(x => x.Value)
                .IsRequired();

            entity.Property(x => x.Unit)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(x => x.MinValue)
                .IsRequired();

            entity.Property(x => x.MaxValue)
                .IsRequired();

            entity.Property(x => x.CreatedAt)
                .IsRequired();

            entity.Property(x => x.UpdatedAt)
                .IsRequired();

            // Foreign Key vers Machine.Id
            entity.Property(x => x.MachineId)
                .IsRequired();

            // Relation :
            // Machine 1 ---- 0..* MachineConfiguration
            entity.HasOne(mc => mc.Machine)
                .WithMany(m => m.MachineConfigurations)
                .HasForeignKey(mc => mc.MachineId);
        });

        // ============================================================
        // SENSOR
        // ============================================================

        builder.Entity<Sensor>(entity =>
        {
            // Primary Key
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(x => x.Type)
                .IsRequired()
                .HasMaxLength(100);

            // Foreign Key vers Machine.Id
            entity.Property(x => x.MachineId)
                .IsRequired();

            // Relation :
            // Machine 1 ---- 0..* Sensor
            entity.HasOne(s => s.Machine)
                .WithMany(m => m.Sensors)
                .HasForeignKey(s => s.MachineId);
        });

        // ============================================================
        // MEASURE
        // ============================================================

        builder.Entity<Measure>(entity =>
        {
            // Primary Key
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Value)
                .IsRequired();

            entity.Property(x => x.Unit)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(x => x.CreatedAt)
                .IsRequired();

            // Foreign Key vers Sensor.Id
            entity.Property(x => x.SensorId)
                .IsRequired();

            // Relation :
            // Sensor 1 ---- 0..* Measure
            entity.HasOne(m => m.Sensor)
                .WithMany(s => s.Measures)
                .HasForeignKey(m => m.SensorId);
        });
    }
}
