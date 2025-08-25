using System.Threading.Tasks;
using CrewBackend.DTOs;

namespace CrewBackend.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardDataDTO> GetDashboardStatsAsync();
    }
}
