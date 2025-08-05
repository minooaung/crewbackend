using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using crewbackend.Models;

namespace crewbackend.Data
{
    public class Seeder
    {
        private readonly AppDbContext _context;
        private readonly IPasswordHasher<User> _hasher;
        private readonly IConfiguration _config; // ✅ Add this line

        public Seeder(AppDbContext context, IPasswordHasher<User> hasher, IConfiguration config)
        {
            _context = context;
            _hasher = hasher;
            _config = config; // ✅ Assign it
        }

        public async Task SeedAsync()
        {
            var now = DateTime.UtcNow;

            // Ensure DB exists & all migrations are applied
            // Therefore I do not need to run "dotnet ef database update" manually anymore
            await _context.Database.MigrateAsync();

            // Seed UserRoles if not present
            if (!await _context.UserRoles.AnyAsync())
            {
                // _context.UserRoles.AddRange(
                //     new UserRole { RoleName = "Admin", CreatedAt = now, UpdatedAt = now },
                //     new UserRole { RoleName = "Employee", CreatedAt = now, UpdatedAt = now }
                // );

                var roles = new List<UserRole>
                {
                    new UserRole { RoleName = "Admin", CreatedAt = now, UpdatedAt = now },
                    new UserRole { RoleName = "Employee", CreatedAt = now, UpdatedAt = now }
                };

                _context.UserRoles.AddRange(roles);
                await _context.SaveChangesAsync();
            }

            // Seed Organisations if not present
            if (!await _context.Organisations.AnyAsync())
            {

                // _context.Organisations.AddRange(
                //     new Organisation { OrgName = "Alpha Corp", CreatedAt = now, UpdatedAt = now },
                //     new Organisation { OrgName = "Beta Inc", CreatedAt = now, UpdatedAt = now }
                // );

                var orgs = new List<Organisation>
                {
                    new Organisation { OrgName = "Alpha Corp", CreatedAt = now, UpdatedAt = now },
                    new Organisation { OrgName = "Beta Inc", CreatedAt = now, UpdatedAt = now }
                };

                _context.Organisations.AddRange(orgs);
                await _context.SaveChangesAsync();
            }

            // Seed Users if not present
            if (!await _context.Users.AnyAsync())
            {
                // Get role IDs dynamically to avoid hardcoding
                var adminRoleId = await _context.UserRoles
                    .Where(r => r.RoleName == "Admin")
                    .Select(r => r.RoleId)
                    .FirstOrDefaultAsync();

                var empRoleId = await _context.UserRoles
                    .Where(r => r.RoleName == "Employee")
                    .Select(r => r.RoleId)
                    .FirstOrDefaultAsync();

                var adminEmail = _config["Seed:AdminEmail"] ?? throw new InvalidOperationException("AdminEmail not configured.");
                var adminPassword = _config["Seed:AdminPassword"] ?? throw new InvalidOperationException("AdminPassword not configured.");

                var empEmail = _config["Seed:EmployeeEmail"] ?? throw new InvalidOperationException("EmployeeEmail not configured.");
                var empPassword = _config["Seed:EmployeePassword"] ?? throw new InvalidOperationException("EmployeePassword not configured.");

                var users = new List<User>
                {
                    new User
                    {                        
                        Name = "Admin",
                        //Email = "admin@example.com",
                        Email = adminEmail,
                        RoleId = adminRoleId,
                        //Password = _hasher.HashPassword(null!, "SecureAdminPassword@888"),
                        Password = _hasher.HashPassword(null!, adminPassword),
                        CreatedAt = now,
                        UpdatedAt = now
                    },
                    new User
                    {                        
                        Name = "Jackie Chan",
                        // Email = "jackie@example.com",
                        Email = empEmail,
                        RoleId = empRoleId,
                        //Password = _hasher.HashPassword(null!, "SecureJackiePassword@888!"),
                        Password = _hasher.HashPassword(null!, empPassword),
                        // Password is now set during initialization, satisfying the required keyword.
                        // null! is acceptable for HashPassword() in seeding logic because it’s only used internally to salt the hash, and no user-specific data is needed for my case.
                        CreatedAt = now,
                        UpdatedAt = now
                    }
                };

                _context.Users.AddRange(users);
                await _context.SaveChangesAsync();
            }
        }
    }
}