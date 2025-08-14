using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CrewBackend.DTOs;
using CrewBackend.Services.Reports;
using crewbackend.Helpers;

namespace CrewBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpPost("generate")]
        public async Task<IActionResult> Generate([FromBody] GenerateReportDTO request)
        {
            if (!ModelState.IsValid)
            {
                ControllerHelpers.HandleModelStateErrors(ModelState);
            }

            var (content, contentType) = await _reportService.GenerateAsync(
                request.ReportType.ToLower(),
                request.OutputFormat.ToLower()
            );

            // For HTML format, return as string
            if (request.OutputFormat.ToLower() == "html")
            {
                return Content(System.Text.Encoding.UTF8.GetString(content), contentType);
            }

            // For JSON format, parse and return as JSON
            if (request.OutputFormat.ToLower() == "json")
            {
                var jsonString = System.Text.Encoding.UTF8.GetString(content);
                return Content(jsonString, contentType);
            }

            // For other formats (PDF, Excel, CSV), return as file download
            var metadata = ReportMetadata.GetForFormat(request.OutputFormat.ToLower());
            return File(content, contentType, $"{request.ReportType}-report.{metadata.Extension}");
        }
    }
}
