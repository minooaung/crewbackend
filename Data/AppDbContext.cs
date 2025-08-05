using Microsoft.EntityFrameworkCore;
using crewbackend.Models;

namespace crewbackend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        // A constructor that allows AppDbContext to receive options (like connection string, etc.) 
        // from outside (usually passed in by ASP.NET Core's dependency injection system).

        // DbContextOptions<AppDbContext> → "The config options specifically for the AppDbContext."
        // <AppDbContext> is the generic type argument that tells EF Core which context the options are for.

        public required DbSet<User> Users { get; set; }
        // Defines a Users table in your database, mapped to a User C# class. 
        // DbSet<User> allows you to query and manipulate User records.
        public required DbSet<UserRole> UserRoles { get; set; }
        public required DbSet<Organisation> Organisations { get; set; }
        public required DbSet<OrganisationUser> OrganisationUsers { get; set; }
        
        
        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            // This method is called when the database model is being created. 
            // Use it to configure things like table names, keys, column settings, relationships, etc.

            // var seedTime = new DateTime(2025, 5, 14, 0, 0, 0, DateTimeKind.Utc);

            // === UserRole ===
            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.ToTable("UserRoles");

                entity.HasKey(e => e.RoleId);
                entity.Property(e => e.RoleName)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.CreatedAt).HasColumnType("datetime2");
                entity.Property(e => e.UpdatedAt).HasColumnType("datetime2");
                
            });

            // === Organisation ===
            modelBuilder.Entity<Organisation>(entity =>
            {
                entity.ToTable("Organisations");

                entity.HasKey(e => e.OrgId);
                entity.Property(e => e.OrgName)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.CreatedAt).HasColumnType("datetime2");
                entity.Property(e => e.UpdatedAt).HasColumnType("datetime2");
                
            });

            // === User ===
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.HasIndex(e => e.Email)
                    .IsUnique();

                entity.Property(e => e.Password)
                    .IsRequired();

                entity.Property(e => e.RememberToken)
                    .HasMaxLength(100)
                    .IsRequired(false);

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime2")
                    .IsRequired(false);

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime2")
                    .IsRequired(false);

                entity.Property(e => e.EmailVerifiedAt)
                    .HasColumnType("datetime2")
                    .IsRequired(false);

                entity.HasOne(e => e.Role)
                    .WithMany(r => r.Users)
                    .HasForeignKey(e => e.RoleId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // === OrganisationUser (Many-to-Many) ===
            modelBuilder.Entity<OrganisationUser>(entity =>
            {
                entity.ToTable("OrganisationUsers");

                entity.HasKey(e => new { e.UserId, e.OrgId });

                entity.HasOne(e => e.User)
                    .WithMany(u => u.OrganisationUsers)
                    .HasForeignKey(e => e.UserId);

                entity.HasOne(e => e.Organisation)
                    .WithMany(o => o.OrganisationUsers)
                    .HasForeignKey(e => e.OrgId);
            });
        }
    }    
}