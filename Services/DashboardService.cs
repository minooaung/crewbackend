using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CrewBackend.Data;
using CrewBackend.DTOs;
using CrewBackend.Models;
using CrewBackend.Services.Interfaces;

namespace CrewBackend.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _context;

        public DashboardService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardDataDTO> GetDashboardStatsAsync()
        {
            var basicStats = await GetBasicStatsAsync();
            var userRoles = await GetUserRoleStatsAsync();
            var growth = await GetGrowthStatsAsync();

            return new DashboardDataDTO
            {
                Stats = basicStats,
                UserRoles = userRoles,
                Growth = growth,
                Meta = new MetaDTO
                {
                    LastUpdated = DateTime.UtcNow.ToString("O")
                }
            };
        }

            private async Task<BasicStatsDTO> GetBasicStatsAsync()
    {
        var totalUsers = await _context.Users.Where(u => !u.IsDeleted).CountAsync();
        var totalOrganizations = await _context.Set<Organisation>().CountAsync();
        var adminUsers = await _context.Users
            .Where(u => !u.IsDeleted && u.Role.RoleName == UserRoleConstants.Admin)
            .CountAsync();
        var superAdminUsers = await _context.Users
            .Where(u => !u.IsDeleted && u.Role.RoleName == UserRoleConstants.SuperAdmin)
            .CountAsync();
        var activeOrganizations = await _context.Set<Organisation>()
            .Include(o => o.OrganisationUsers)
            .Where(o => o.OrganisationUsers.Any())
            .CountAsync();

        return new BasicStatsDTO
        {
            TotalUsers = totalUsers,
            TotalOrganizations = totalOrganizations,
            AdminUsers = adminUsers,
            SuperAdminUsers = superAdminUsers,
            ActiveOrganizations = activeOrganizations
        };
    }

    private async Task<UserRolesDTO> GetUserRoleStatsAsync()
    {
        var superAdminCount = await _context.Users
            .Where(u => !u.IsDeleted && u.Role.RoleName == UserRoleConstants.SuperAdmin)
            .CountAsync();

        var adminCount = await _context.Users
            .Where(u => !u.IsDeleted && u.Role.RoleName == UserRoleConstants.Admin)
            .CountAsync();

        var employeeCount = await _context.Users
            .Where(u => !u.IsDeleted && u.Role.RoleName == UserRoleConstants.Employee)
            .CountAsync();

        return new UserRolesDTO
        {
            SuperAdminCount = superAdminCount,
            AdminCount = adminCount,
            EmployeeCount = employeeCount
        };
    }

    private async Task<GrowthStatsDTO> GetGrowthStatsAsync()
    {
        var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);

        var userGrowth = await _context.Users
            .Where(u => !u.IsDeleted && u.CreatedAt != null && u.CreatedAt >= sixMonthsAgo)
            .GroupBy(u => u.CreatedAt!.Value.Month)
            .Select(g => new { Month = g.Key, Count = g.Count() })
            .OrderBy(x => x.Month)
            .ToListAsync();

        var orgGrowth = await _context.Set<Organisation>()
            .Where(o => o.CreatedAt >= sixMonthsAgo)
            .GroupBy(o => o.CreatedAt.Month)
            .Select(g => new { Month = g.Key, Count = g.Count() })
            .OrderBy(x => x.Month)
            .ToListAsync();

        // Convert month numbers to month names
        var monthNames = userGrowth.Select(x => new DateTime(DateTime.UtcNow.Year, x.Month, 1).ToString("MMM")).ToArray();

        return new GrowthStatsDTO
        {
            Labels = monthNames,
            Users = userGrowth.Select(x => x.Count).ToArray(),
                Organizations = orgGrowth.Select(x => x.Count).ToArray()
            };
        }
    }
}
