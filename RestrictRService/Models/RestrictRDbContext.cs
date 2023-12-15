﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace RestrictRService.Models;

public partial class RestrictRDbContext : DbContext
{
    public RestrictRDbContext(DbContextOptions<RestrictRDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BlockedApplication> BlockedApplications { get; set; }

    public virtual DbSet<BlockedWebsite> BlockedWebsites { get; set; }

    public virtual DbSet<BlockedWebsiteUrl> BlockedWebsiteUrls { get; set; }

    public virtual DbSet<Event> Events { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BlockedApplication>(entity =>
        {
            entity.HasKey(e => e.BlockedAppsId);

            entity.HasIndex(e => e.BlockedAppsId, "IX_BlockedApplications_BlockedAppsId").IsUnique();

            entity.Property(e => e.Comments).IsRequired();
            entity.Property(e => e.DisplayName).IsRequired();
            entity.Property(e => e.DisplayVersion).IsRequired();
            entity.Property(e => e.InstallDate).IsRequired();
            entity.Property(e => e.InstallLocation).IsRequired();
            entity.Property(e => e.Publisher).IsRequired();
            entity.Property(e => e.RegistryPath).IsRequired();
            entity.Property(e => e.UninstallString).IsRequired();

            entity.HasOne(d => d.Event).WithMany(p => p.BlockedApplications)
                .HasForeignKey(d => d.EventId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<BlockedWebsite>(entity =>
        {
            entity.HasKey(e => e.BlockedSitesId);

            entity.HasIndex(e => e.BlockedSitesId, "IX_BlockedWebsites_BlockedSitesId").IsUnique();

            entity.HasOne(d => d.Event).WithMany(p => p.BlockedWebsites)
                .HasForeignKey(d => d.EventId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<BlockedWebsiteUrl>(entity =>
        {
            entity.HasKey(e => e.BlockedSitesUrlsId);

            entity.HasIndex(e => e.BlockedSitesUrlsId, "IX_BlockedWebsiteUrls_BlockedSitesUrlsId").IsUnique();

            entity.Property(e => e.Url).IsRequired();

            entity.HasOne(d => d.BlockedSites).WithMany(p => p.BlockedWebsiteUrls)
                .HasForeignKey(d => d.BlockedSitesId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasIndex(e => e.EventId, "IX_Events_EventId").IsUnique();

            entity.Property(e => e.Duration).IsRequired();
            entity.Property(e => e.Start).IsRequired();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}