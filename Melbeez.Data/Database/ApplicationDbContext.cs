using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Melbeez.Common.Services.Abstraction;
using Melbeez.Domain.Entities.Identity;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace Melbeez.Data.Database
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
    {
        private readonly IUserContextService userContextService;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IUserContextService userContextService)
            : base(options)
        {
            this.userContextService = userContextService;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ////Set Rules for ApplicationRole properties
            //var applicationRoleEntity = modelBuilder.Entity<ApplicationRole>();

            modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
            base.OnModelCreating(modelBuilder);
        }

        public async override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            OnBeforeSaveChanges();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void OnBeforeSaveChanges()
        {
            base.ChangeTracker.DetectChanges();
            var now = DateTime.UtcNow;
            string userId = userContextService?.GetUserId() ?? "-1";

            foreach (EntityEntry entry in base.ChangeTracker.Entries())
            {
                if (entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                {
                    continue;
                }

                if (entry.State == EntityState.Added && entry.Metadata.FindProperty("CreatedOn") != null)
                {
                    entry.Property("CreatedOn").CurrentValue = now;
                    entry.Property("CreatedBy").CurrentValue = userId;
                }

                if (entry.State == EntityState.Modified && entry.Metadata.FindProperty("UpdatedOn") != null)
                {
                    entry.Property("UpdatedOn").CurrentValue = now;
                    entry.Property("UpdatedBy").CurrentValue = userId;
                }
            }
        }
    }
}
