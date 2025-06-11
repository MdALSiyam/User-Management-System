using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UserManagementSystem.Models;

namespace UserManagementSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Email)
                      .IsUnique()
                      .HasDatabaseName("IX_Users_Email");

                entity.Property(u => u.Name)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(u => u.Email)
                      .IsRequired()
                      .HasMaxLength(255);

                entity.Property(u => u.Status)
                      .IsRequired()
                      .HasMaxLength(20)
                      .HasDefaultValue("Active");

                entity.Property(u => u.RegistrationTime)
                      .HasDefaultValueSql("GETUTCDATE()");
            });

            modelBuilder.Entity<IdentityUser>(entity =>
            {
                entity.Property(u => u.Email).HasMaxLength(255);
                entity.Property(u => u.NormalizedEmail).HasMaxLength(255);
                entity.Property(u => u.UserName).HasMaxLength(100);
                entity.Property(u => u.NormalizedUserName).HasMaxLength(100);
            });
        }
    }
}