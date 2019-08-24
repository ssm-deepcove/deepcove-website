﻿using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Deepcove_Trust_Website.Models;
using System.Threading.Tasks;
using System.Threading;

namespace Deepcove_Trust_Website.Data
{
    public class WebsiteDataContext : DbContext
    {
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Link> CmsLink { get; set; }
        public DbSet<PasswordReset> PasswordResets { get; set; }
        public DbSet<Page> Pages { get; set; }
        public DbSet<PageRevision> PageRevisions { get; set; }
        public DbSet<TextField> TextField { get; set; }
        public DbSet<Template> PageTemplates { get; set; }
        public DbSet<SystemSettings> SystemSettings { get; set; }
        public DbSet<NotificationChannel> NotificationChannels { get; set; }


        public WebsiteDataContext(DbContextOptions<WebsiteDataContext> options) : base(options)
        {
            // Temporarily disabled as it seems to prevent database rollbacks.
            // Auto runs migrations
            //Database.Migrate();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string dburl = Environment.GetEnvironmentVariable("DATABASE_URL");
            if (!string.IsNullOrEmpty(dburl))
            {
                // Use the URL from the docker file
                optionsBuilder.UseSqlServer(dburl);
            }
            else
            {
                // appsettings.json
                base.OnConfiguring(optionsBuilder);
            }
        }


        // Adds timestamps to database record(s)
        public override int SaveChanges()
        {
            AddTimestamps();
            return base.SaveChanges();
        }

        //Adds timestamps to database record(s)
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            AddTimestamps();
            return await base.SaveChangesAsync();
        }

        // Adds timestamps to database record(s)
        private void AddTimestamps()
        {
            var entities = ChangeTracker.Entries()
                .Where(x => x.Entity is BaseEntity && (x.State == EntityState.Added || x.State == EntityState.Modified));

            foreach (var entity in entities)
            {
                var now = DateTime.UtcNow;

                if (entity.State == EntityState.Added)
                {
                    ((BaseEntity)entity.Entity).CreatedAt = now;
                }
                ((BaseEntity)entity.Entity).UpdatedAt = now;
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>().HasQueryFilter(f => f.DeletedAt == null);
            modelBuilder.Entity<Page>().HasQueryFilter(f => f.DeletedAt == null);

            // Enum Conversions
            modelBuilder.Entity<Page>().Property(p => p.Section).HasConversion(c => (int)c, c => (Section)c);
            modelBuilder.Entity<Page>().Property(p => p.QuickLink).HasConversion(c => (int)c, c => (QuickLinkSection)c);
            modelBuilder.Entity<Link>().Property(p => p.Align).HasConversion(c => (int)c, c => (Align)c);
            modelBuilder.Entity<Link>().Property(p => p.Color).HasConversion(c => (int)c, c => (Color)c);
            // End Enum Conversions

            // Define keys for junction tables
            modelBuilder.Entity<ChannelMembership>()
                .HasKey(e => new { e.AccountId, e.NotificationChannelId });

            modelBuilder.Entity<RevisionTextField>()
                .HasKey(e => new { e.PageRevisionId, e.TextFieldId });

            // Place unique constraints onto appropriate properties

            modelBuilder.Entity<Template>()
                .HasIndex(e => e.Name).IsUnique();

            modelBuilder.Entity<Page>()
                .HasIndex(e => e.Name).IsUnique();
        }
    }
}