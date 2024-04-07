using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Ford.WebApi.Data.Entities;

namespace Ford.WebApi.Data;

public class FordContext : IdentityDbContext<User, IdentityRole<long>, long>
{
    public FordContext() : base()
    {
    }

    public FordContext(DbContextOptions<FordContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Horse> Horses { get; set; } = null!;
    public virtual DbSet<Save> Saves { get; set; } = null!;
    public virtual DbSet<SaveBone> SaveBones { get; set; } = null!;
    public virtual DbSet<HorseUser> HorseUsers { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string connection = "Server=RUFIKDESKTOP;Database=db-ford;User=dataworker;Password=Rufik2024;" +
                            "TrustServerCertificate=True;" +
                            "MultipleActiveResultSets=True;";
        optionsBuilder.UseSqlServer(connection);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(x => x.Id).ValueGeneratedOnAdd();

            entity.HasIndex(x => x.UserName)
                .HasDatabaseName("IX_UserNames")
                .IsUnique();

            entity.HasIndex(x => x.NormalizedUserName)
                .HasDatabaseName("IX_NormalizedUserNames")
                .IsUnique();

            entity.HasIndex(x => x.Email)
                .HasDatabaseName("IX_UserEmails");

            entity.HasIndex(x => x.NormalizedEmail)
                .HasDatabaseName("IX_NormalizedUserEmails");

            entity.HasIndex(x => x.FirstName)
                .HasDatabaseName("IX_UserFirstNames");

            entity.HasIndex(x => x.LastName)
                .HasDatabaseName("IX_UserLastNames");
        });

        modelBuilder.Entity<Horse>(entity =>
        {
            entity.Property(e => e.HorseId).ValueGeneratedOnAdd();

            entity.HasMany(x => x.Users)
                .WithMany(x => x.Horses)
                .UsingEntity<HorseUser>(
                    l => l.HasOne(x => x.User).WithMany(x => x.HorseUsers).HasForeignKey(x => x.UserId),
                    r => r.HasOne(x => x.Horse).WithMany(x => x.HorseUsers).HasForeignKey(x => x.HorseId));

            entity.HasOne(e => e.LastModifiedByUser)
                .WithMany()
                .HasForeignKey(k => k.LastModifiedByUserId);

            entity.HasIndex(x => x.Name)
                .HasDatabaseName("IX_HorseNames");
        });

        modelBuilder.Entity<HorseUser>(entity =>
        {
            entity.HasIndex(x => x.AccessRole).HasDatabaseName("IX_HorseUserAccessRoles");
        });

        modelBuilder.Entity<Save>(entity =>
        {
            entity.Property(e => e.SaveId).ValueGeneratedOnAdd();

            entity.HasOne(d => d.Horse)
                .WithMany(p => p.Saves)
                .HasForeignKey(d => d.HorseId);

            entity.HasOne(d => d.CreatedByUser)
                .WithMany()
                .HasForeignKey(d => d.CreatedByUserId);

            entity.HasOne(d => d.LastUpdatedByUser)
                .WithMany()
                .HasForeignKey(k => k.LastModifiedByUserId);
        });

        modelBuilder.Entity<SaveBone>(entity =>
        {
            entity.HasKey(e => new { e.SaveBoneId });

            entity.HasOne(d => d.Save)
                .WithMany(p => p.SaveBones)
                .HasForeignKey(d => d.SaveId)
                .IsRequired();
        });
    }
}
