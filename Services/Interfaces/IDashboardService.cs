using System.Threading.Tasks;
using crewbackend.DTOs;

namespace crewbackend.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardDataDTO> GetDashboardStatsAsync();
    }
}
