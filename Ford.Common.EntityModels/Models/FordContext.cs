using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Ford.EntityModels
{
    public partial class FordContext : DbContext
    {
        public FordContext()
        {
        }

        public FordContext(DbContextOptions<FordContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Bone> Bones { get; set; } = null!;
        public virtual DbSet<Horse> Horses { get; set; } = null!;
        public virtual DbSet<HorseSafe> HorseSaves { get; set; } = null!;
        public virtual DbSet<User> Users { get; set; } = null!;
        public virtual DbSet<Vector> Vectors { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlite("filename=../Ford.db");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Horse>(entity =>
            {
                entity.Property(e => e.HorseId).ValueGeneratedNever();

                entity.HasMany(d => d.Users)
                    .WithMany(p => p.Horses)
                    .UsingEntity<Dictionary<string, object>>(
                        "HorseOwner",
                        l => l.HasOne<User>().WithMany().HasForeignKey("UserId").OnDelete(DeleteBehavior.ClientSetNull),
                        r => r.HasOne<Horse>().WithMany().HasForeignKey("HorseId").OnDelete(DeleteBehavior.ClientSetNull),
                        j =>
                        {
                            j.HasKey("HorseId", "UserId");

                            j.ToTable("HorseOwner");
                        });
            });

            modelBuilder.Entity<HorseSafe>(entity =>
            {
                entity.Property(e => e.HorseSaveId).ValueGeneratedNever();
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.UserId).ValueGeneratedNever();
            });

            modelBuilder.Entity<Vector>(entity =>
            {
                entity.Property(e => e.VectorId).ValueGeneratedNever();

                entity.Property(e => e.X).HasDefaultValueSql("0");

                entity.Property(e => e.Y).HasDefaultValueSql("0");

                entity.Property(e => e.Z).HasDefaultValueSql("0");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
