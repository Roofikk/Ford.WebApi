using System;
using System.Collections.Generic;
using Ford.EntityModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Ford.DataContext.Sqlite
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
        public virtual DbSet<HorseSave> HorseSaves { get; set; } = null!;
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
            modelBuilder.Entity<User>()
                .HasMany(c => c.Horses)
                .WithMany(s => s.Users)
                .UsingEntity(j => j.ToTable("HorseOwner"));

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
