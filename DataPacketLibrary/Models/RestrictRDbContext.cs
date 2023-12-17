using System;
using System.Collections.Generic;
using Laraue.EfCoreTriggers.Common.Extensions;
using Microsoft.EntityFrameworkCore;
using static System.Environment;

namespace DataPacketLibrary.Models;

public partial class RestrictRDbContext : DbContext
{
    public RestrictRDbContext(DbContextOptions<RestrictRDbContext> options)
        : base(options) {}

    public virtual DbSet<ApplicationInfo> BlockedApplications { get; set; } = null!;

    public virtual DbSet<BlockedWebsites> BlockedWebsites { get; set; } = null!;

    public virtual DbSet<BlockedWebsiteUrl> BlockedWebsiteUrls { get; set; } = null!;

    public virtual DbSet<Event> Events { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.EventId);

            entity.OwnsOne(e => e.BlockedSites);

            entity.HasMany(e => e.BlockedApps)
            .WithOne()
            .HasForeignKey(e => e.EventId);
        });

        modelBuilder.Entity<BlockedWebsites>(entity =>
        {

            entity.HasMany(e => e.BlockedWebsiteUrls)
            .WithOne()
            .HasForeignKey(e => e.BlockedWebsiteId);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string databaseFilename = "RestrictR_DB.db";
        string programDataPath = GetFolderPath(SpecialFolder.CommonApplicationData);
        string databasePath = Path.Combine(programDataPath, "RestrictR", databaseFilename);

        Directory.CreateDirectory(databasePath);

        string connectionString = $"Data Source={databasePath}";
        optionsBuilder.UseSqlite(connectionString);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
