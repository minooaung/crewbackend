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

        //public DbSet<User> Users { get; set; } = null!;
        public required DbSet<User> Users { get; set; }
        // Defines a Users table in your database, mapped to a User C# class. 
        // DbSet<User> allows you to query and manipulate User records.
        
        protected override void OnModelCreating(ModelBuilder modelBuilder){
            // This method is called when the database model is being created. 
            // Use it to configure things like table names, keys, column settings, relationships, etc.

            modelBuilder.Entity<User>(entity => { // This line starts configuring the User entity.

                entity.ToTable("Users"); // Maps the User entity to the "Users" table in your database.

                entity.HasKey(e => e.Id); // Sets the primary key of the table to the Id property.

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(255);
                
                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.HasIndex(e => e.Email)
                    .IsUnique(); // Adds a unique constraint on the Email field, so two users can’t have the same email.

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

                entity.HasData(
                    new User
                    {
                        Id = 1,
                        Name = "John Doe",
                        Email = "john@example.com",
                        Password = "hashed-password", // Replace with bcrypt-style hash later
                        EmailVerifiedAt = null,
                        RememberToken = null,
                        CreatedAt = new DateTime(2024, 1, 1, 12, 0, 0),
                        UpdatedAt = new DateTime(2024, 1, 1, 12, 0, 0)
                    },
                    new User
                    {
                        Id = 2,
                        Name = "Jane Smith",
                        Email = "jane@example.com",
                        Password = "hashed-password",
                        // EmailVerifiedAt = DateTime.UtcNow,
                        EmailVerifiedAt = null,
                        RememberToken = null,
                        CreatedAt = new DateTime(2024, 1, 1, 13, 0, 0),
                        UpdatedAt = new DateTime(2024, 1, 1, 13, 0, 0)
                    }
                );
            });
        }
    }

    // public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    // {
    //     // A constructor that allows AppDbContext to receive options (like connection string, etc.) 
    //     // from outside (usually passed in by ASP.NET Core's dependency injection system).

    //     // DbContextOptions<AppDbContext> → "The config options specifically for the AppDbContext."
    //     // <AppDbContext> is the generic type argument that tells EF Core which context the options are for.

    //     public DbSet<User>? Users { get;}
    //     // Defines a Users table in your database, mapped to a User C# class. 
    //     // DbSet<User> allows you to query and manipulate User records.
        
    //     protected override void OnModelCreating(ModelBuilder modelBuilder){
    //         // This method is called when the database model is being created. 
    //         // Use it to configure things like table names, keys, column settings, relationships, etc.

    //         modelBuilder.Entity<User>(entity => { // This line starts configuring the User entity.

    //             entity.ToTable("Users"); // Maps the User entity to the "Users" table in your database.

    //             entity.HasKey(e => e.Id); // Sets the primary key of the table to the Id property.
    //             entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                
    //             entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
    //             entity.HasIndex(e => e.Email).IsUnique(); // Adds a unique constraint on the Email field, so two users can’t have the same email.

    //             entity.Property(e => e.Password).IsRequired();
    //             entity.Property(e => e.RememberToken).IsRequired();

    //             entity.Property(e => e.CreatedAt).HasColumnType("datetime2");
    //             entity.Property(e => e.UpdatedAt).HasColumnType("datetime2");
    //         });
    //     }
    // }
}