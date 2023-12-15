using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace RestrictRService.Models
{
    public partial class RestrictR_DBContext : DbContext
    {
        public RestrictR_DBContext()
        {
        }

        public RestrictR_DBContext(DbContextOptions<RestrictR_DBContext> options)
            : base(options)
        {
        }

        public virtual DbSet<BlockedApplication> BlockedApplications { get; set; } = null!;
        public virtual DbSet<BlockedWebsite> BlockedWebsites { get; set; } = null!;
        public virtual DbSet<BlockedWebsiteUrl> BlockedWebsiteUrls { get; set; } = null!;
        public virtual DbSet<Event> Events { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("Data Source=RestrictR_DB.db");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BlockedApplication>(entity =>
            {
                entity.HasNoKey();

                entity.HasOne(d => d.Event)
                    .WithMany()
                    .HasForeignKey(d => d.EventId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<BlockedWebsite>(entity =>
            {
                entity.HasNoKey();

                entity.Property(e => e.BlockAllSites).HasColumnType("BOOLEAN");

                entity.HasOne(d => d.Event)
                    .WithMany()
                    .HasForeignKey(d => d.EventId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<BlockedWebsiteUrl>(entity =>
            {
                entity.HasNoKey();
            });

            modelBuilder.Entity<Event>(entity =>
            {
                entity.HasIndex(e => e.EventId, "IX_Events_EventId")
                    .IsUnique();

                entity.Property(e => e.Duration).HasColumnType("TIME");

                entity.Property(e => e.Start).HasColumnType("DATETIME");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
