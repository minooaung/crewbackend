using System;

namespace crewbackend.DTOs
{
    public class DashboardStatsDTO
    {
        public bool Success { get; set; }
        public DashboardDataDTO? Data { get; set; }
    }

    public class DashboardDataDTO
    {
        required public BasicStatsDTO Stats { get; set; }
        required public UserRolesDTO UserRoles { get; set; }
        required public GrowthStatsDTO Growth { get; set; }
        required public MetaDTO Meta { get; set; }
    }

    public class BasicStatsDTO
    {
        public int TotalUsers { get; set; }
        public int TotalOrganizations { get; set; }
        public int AdminUsers { get; set; }
        public int SuperAdminUsers { get; set; }
        public int ActiveOrganizations { get; set; }
    }

    public class UserRolesDTO
    {
        public int SuperAdminCount { get; set; }
        public int AdminCount { get; set; }
        public int EmployeeCount { get; set; }
    }

    public class GrowthStatsDTO
    {
        required public string[] Labels { get; set; }
        required public int[] Users { get; set; }
        required public int[] Organizations { get; set; }
    }

    public class MetaDTO
    {
        required public string LastUpdated { get; set; }
    }
}
