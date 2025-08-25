using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CrewBackend.DTOs;
using CrewBackend.Services.Interfaces;

namespace CrewBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        /// <summary>
        /// Get dashboard statistics and analytics data
        /// </summary>
        /// <returns>Dashboard statistics including user counts, organization data, and growth metrics</returns>
        /// <response code="200">Returns the dashboard statistics</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpGet("stats")]
        [ProducesResponseType(typeof(DashboardStatsDTO), 200)]
        public async Task<IActionResult> GetStats()
        {
            try
            {
                var data = await _dashboardService.GetDashboardStatsAsync();
                return Ok(new DashboardStatsDTO
                {
                    Success = true,
                    Data = data
                });
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.Error.WriteLine($"Error fetching dashboard stats: {ex.Message}");
                return StatusCode(500, new DashboardStatsDTO
                {
                    Success = false,
                    Data = null
                });
            }
        }
    }
}
