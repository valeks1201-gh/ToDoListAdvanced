using DAL.Models.Interfaces;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using DAL.Models;
using DAL.Models.Interfaces;

namespace DAL
{
    public class ApplicationDbContext : IdentityDbContext<Account, ApplicationRole, string>
    {
        public string CurrentUserId { get; set; }
        public DbSet<ToDoItem> ToDoItems { get; set; }
        public DbSet<Priority> Priorities { get; set; }
        //public DbSet<User> Users { get; set; }
      
        //public ApplicationDbContext(DbContextOptions options) : base(options)
        //{ }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            builder.Entity<Account>().HasMany(u => u.Claims).WithOne().HasForeignKey(c => c.UserId).IsRequired().OnDelete(DeleteBehavior.Cascade);
            builder.Entity<Account>().HasMany(u => u.Roles).WithOne().HasForeignKey(r => r.UserId).IsRequired().OnDelete(DeleteBehavior.Cascade);
            builder.Entity<Account>().HasMany(u => u.ToDoItems).WithOne().HasForeignKey(t => t.UserId).IsRequired().OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ApplicationRole>().HasMany(r => r.Claims).WithOne().HasForeignKey(c => c.RoleId).IsRequired().OnDelete(DeleteBehavior.Cascade);
            builder.Entity<ApplicationRole>().HasMany(r => r.Users).WithOne().HasForeignKey(r => r.RoleId).IsRequired().OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Priority>().Property(p => p.Name).IsRequired().HasMaxLength(300);
            builder.Entity<Priority>().HasIndex(p => new { p.Name, p.Organization }).IsUnique();
            builder.Entity<Priority>().ToTable($"App{nameof(this.Priorities)}");

            //builder.Entity<User>().Property(c => c.Name).IsRequired().HasMaxLength(300);
            //builder.Entity<User>().HasIndex(c => c.Name);
            //builder.Entity<User>().ToTable($"App{nameof(this.Users)}");

            builder.Entity<ToDoItem>()
                .HasOne(x => x.Priority)
                .WithMany(x => x.ToDoItems)
                .HasForeignKey(x => x.PriorityId).IsRequired().OnDelete(DeleteBehavior.Cascade);
            builder.Entity<ToDoItem>()
               .HasOne(x => x.User)
               .WithMany(x => x.ToDoItems)
               .HasForeignKey(x => x.UserId).IsRequired().OnDelete(DeleteBehavior.Cascade);
            builder.Entity<ToDoItem>().ToTable($"App{nameof(this.ToDoItems)}");
        }

        public override int SaveChanges()
        {
            UpdateAuditEntities();
            return base.SaveChanges();
        }
        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            UpdateAuditEntities();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            UpdateAuditEntities();
            return base.SaveChangesAsync(cancellationToken);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken))
        {
            UpdateAuditEntities();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void UpdateAuditEntities()
        {
            var modifiedEntries = ChangeTracker.Entries()
                .Where(x => x.Entity is IAuditableEntity && (x.State == EntityState.Added || x.State == EntityState.Modified));

            foreach (var entry in modifiedEntries)
            {
                var entity = (IAuditableEntity)entry.Entity;
                DateTime now = DateTime.UtcNow;

                if (entry.State == EntityState.Added)
                {
                    if (CurrentUserId != null)
                    {
                        entity.CreatedDate = now;
                        entity.CreatedBy = CurrentUserId;
                    }
                }
                else
                {
                    base.Entry(entity).Property(x => x.CreatedBy).IsModified = false;
                    base.Entry(entity).Property(x => x.CreatedDate).IsModified = false;
                }

                if (CurrentUserId != null)
                {
                    entity.UpdatedDate = now;
                    entity.UpdatedBy = CurrentUserId;
                }
            }
        }
    }
}
