using System.ComponentModel.DataAnnotations;

namespace CrewBackend.DTOs
{
    public class GenerateReportDTO
    {
        [Required]
        [RegularExpression("^(users|organisations)$", ErrorMessage = "Report type must be either 'users' or 'organisations'")]
        public string ReportType { get; set; } = null!;

        [Required]
        [RegularExpression("^(pdf|excel|csv|json|html)$", ErrorMessage = "Output format must be one of: pdf, excel, csv, json, html")]
        public string OutputFormat { get; set; } = null!;
    }
}
