using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CrewBackend.Models;

namespace CrewBackend.Data
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

                // Configure soft delete properties
                entity.Property(e => e.IsDeleted)
                    .IsRequired()
                    .HasDefaultValue(false);

                // Add index on IsDeleted since we frequently filter on it
                entity.HasIndex(e => e.IsDeleted);

                entity.Property(e => e.DeletedAt)
                    .HasColumnType("datetime2")
                    .IsRequired(false);

                entity.HasOne(e => e.DeletedByUser)
                    .WithMany()
                    .HasForeignKey(e => e.DeletedByUserId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(false);
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

                // Create a filtered unique index that only applies to non-deleted records
                entity.HasIndex(e => e.Email)
                    .IsUnique()
                    .HasFilter("[IsDeleted] = 0");

                entity.Property(e => e.Password)
                    .IsRequired();

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime2")
                    .IsRequired(false);

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime2")
                    .IsRequired(false);

                // Configure soft delete properties
                entity.Property(e => e.IsDeleted)
                    .IsRequired()
                    .HasDefaultValue(false);

                // Add index on IsDeleted since we frequently filter on it
                entity.HasIndex(e => e.IsDeleted);

                entity.Property(e => e.DeletedAt)
                    .HasColumnType("datetime2")
                    .IsRequired(false);

                entity.HasOne(e => e.DeletedByUser)
                    .WithMany()
                    .HasForeignKey(e => e.DeletedByUserId)
                    .OnDelete(DeleteBehavior.Restrict)
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

                entity.HasKey(e => e.Id);

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime2")
                    .IsRequired(false);

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime2")
                    .IsRequired(false);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.OrganisationUsers)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Organisation)
                    .WithMany(o => o.OrganisationUsers)
                    .HasForeignKey(e => e.OrganisationId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.AssignedByUser)
                    .WithMany()
                    .HasForeignKey(e => e.AssignedBy)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(false);

                // Configure soft delete properties
                entity.Property(e => e.IsDeleted)
                    .IsRequired()
                    .HasDefaultValue(false);

                entity.Property(e => e.DeletedAt)
                    .HasColumnType("datetime2")
                    .IsRequired(false);

                // Add index on IsDeleted since we frequently filter on it
                entity.HasIndex(e => e.IsDeleted);

                entity.HasOne(e => e.DeletedByUser)
                    .WithMany()
                    .HasForeignKey(e => e.DeletedByUserId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(false);
            });
        }
    }    
}

/*
Personal Notes:
AppDbContext.cs is properly configured with:

1. User Entity:
- Filtered unique index on Email that only applies to non-deleted records
- Index on IsDeleted for better query performance
- Proper soft delete property configurations
- Correct DeletedByUser relationship

2. OrganisationUser Entity:
- Index on IsDeleted for better query performance
- Proper soft delete property configurations
- Correct DeletedByUser relationship
- Proper relationships with User and Organisation

These configurations ensure:
- Email uniqueness is properly enforced only among active users
- Queries filtering on IsDeleted will be efficient due to the indexes
- All soft delete properties have proper defaults and constraints
- All relationships are properly configured with appropriate delete behaviors

---------------------------------------------------------------------------------------------
entity.HasOne(e => e.DeletedByUser)
	.WithMany()
	.HasForeignKey(e => e.DeletedByUserId)
	.OnDelete(DeleteBehavior.Restrict)
	.IsRequired(false);

This is crucial because:
- It sets up the foreign key relationship to the User who performed the deletion
- OnDelete(DeleteBehavior.Restrict) prevents cascading deletes that could break data integrity
- IsRequired(false) allows the DeletedByUser to be null when not deleted
Without these configurations in AppDbContext:
- we might get unexpected null reference exceptions
- The database schema might not match our expectations
- Foreign key relationships might not work correctly
- Default values wouldn't be automatically set
- Database queries might not perform optimally due to incorrect column types
Think of AppDbContext.cs as the blueprint for our database. 
While our model classes (User.cs, OrganisationUser.cs) define the properties, 
AppDbContext.cs defines how those properties should be stored and behave in the database.
*/