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
                    new UserRole { RoleName = UserRoleConstants.SuperAdmin, CreatedAt = now, UpdatedAt = now },
                    new UserRole { RoleName = UserRoleConstants.Admin, CreatedAt = now, UpdatedAt = now },
                    new UserRole { RoleName = UserRoleConstants.Employee, CreatedAt = now, UpdatedAt = now }
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
                var superAdminRoleId = await _context.UserRoles
                    .Where(r => r.RoleName == UserRoleConstants.SuperAdmin)
                    .Select(r => r.RoleId)
                    .FirstOrDefaultAsync();

                var adminRoleId = await _context.UserRoles
                    .Where(r => r.RoleName == UserRoleConstants.Admin)
                    .Select(r => r.RoleId)
                    .FirstOrDefaultAsync();

                var empRoleId = await _context.UserRoles
                    .Where(r => r.RoleName == UserRoleConstants.Employee)
                    .Select(r => r.RoleId)
                    .FirstOrDefaultAsync();

                // Get Super Admin credentials from configuration
                var superAdmin1Email = _config["Seed:SuperAdmin1Email"] ?? throw new InvalidOperationException("SuperAdmin1Email not configured.");
                var superAdmin1Password = _config["Seed:SuperAdmin1Password"] ?? throw new InvalidOperationException("SuperAdmin1Password not configured.");
                var superAdmin2Email = _config["Seed:SuperAdmin2Email"] ?? throw new InvalidOperationException("SuperAdmin2Email not configured.");
                var superAdmin2Password = _config["Seed:SuperAdmin2Password"] ?? throw new InvalidOperationException("SuperAdmin2Password not configured.");

                var adminEmail = _config["Seed:AdminEmail"] ?? throw new InvalidOperationException("AdminEmail not configured.");
                var adminPassword = _config["Seed:AdminPassword"] ?? throw new InvalidOperationException("AdminPassword not configured.");

                var empEmail = _config["Seed:EmployeeEmail"] ?? throw new InvalidOperationException("EmployeeEmail not configured.");
                var empPassword = _config["Seed:EmployeePassword"] ?? throw new InvalidOperationException("EmployeePassword not configured.");

                var users = new List<User>
                {
                    new User
                    {
                        Name = "Super Admin 1",
                        Email = superAdmin1Email,
                        RoleId = superAdminRoleId,
                        Password = _hasher.HashPassword(null!, superAdmin1Password),
                        CreatedAt = now,
                        UpdatedAt = now
                    },
                    new User
                    {
                        Name = "Super Admin 2",
                        Email = superAdmin2Email,
                        RoleId = superAdminRoleId,
                        Password = _hasher.HashPassword(null!, superAdmin2Password),
                        CreatedAt = now,
                        UpdatedAt = now
                    },
                    new User
                    {                        
                        Name = "Admin",
                        Email = adminEmail,
                        RoleId = adminRoleId,
                        Password = _hasher.HashPassword(null!, adminPassword),
                        CreatedAt = now,
                        UpdatedAt = now
                    },
                    new User
                    {                        
                        Name = "Jackie Chan",
                        Email = empEmail,
                        RoleId = empRoleId,
                        Password = _hasher.HashPassword(null!, empPassword),
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