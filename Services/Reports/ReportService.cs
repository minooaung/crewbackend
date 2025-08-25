using CrewBackend.Data;
using CrewBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace CrewBackend.Services.Reports
{
    public class ReportService : IReportService
    {
        private readonly AppDbContext _context;
        private readonly ReportFormatterFactory _formatterFactory;

        private readonly Dictionary<string, Dictionary<string, string>> _columnsMap = new()
        {
            ["users"] = new Dictionary<string, string>
            {
                ["Name"] = "Name",
                ["Email"] = "Email",
                ["Role"] = "Role.RoleName",
                ["Created At"] = "CreatedAt"
            },
            ["organisations"] = new Dictionary<string, string>
            {
                ["Name"] = "OrgName",
                ["Created At"] = "CreatedAt",
                ["Assigned Users"] = "users" // Special handling in formatter
            }
        };

        public ReportService(AppDbContext context, ReportFormatterFactory formatterFactory)
        {
            _context = context;
            _formatterFactory = formatterFactory;
        }

        public async Task<(byte[] Content, string ContentType)> GenerateAsync(string reportType, string outputFormat)
        {
            if (!_columnsMap.ContainsKey(reportType))
            {
                throw new ArgumentException($"Invalid report type: {reportType}");
            }

            var formatter = _formatterFactory.GetFormatter(outputFormat);
            var columns = _columnsMap[reportType];

            return reportType switch
            {
                "users" => await GenerateUsersReport(formatter, columns),
                "organisations" => await GenerateOrganisationsReport(formatter, columns),
                _ => throw new ArgumentException($"Unsupported report type: {reportType}")
            };
        }

        private async Task<(byte[] Content, string ContentType)> GenerateUsersReport(
            IReportFormatter formatter,
            Dictionary<string, string> columns)
        {
            var users = await _context.Users
                .Include(u => u.Role)
                .Where(u => !u.IsDeleted)  // Exclude soft-deleted users
                .OrderBy(u => u.Id)
                .ToListAsync();

            return formatter.Format(
                "users",
                columns,
                users
            );
        }

        private async Task<(byte[] Content, string ContentType)> GenerateOrganisationsReport(
            IReportFormatter formatter,
            Dictionary<string, string> columns)
        {
            var organisations = await _context.Organisations
                .Include(o => o.OrganisationUsers.Where(ou => !ou.IsDeleted))
                    .ThenInclude(ou => ou.User)
                        .ThenInclude(u => u.Role)
                .Where(o => !o.IsDeleted)  // Exclude soft-deleted organisations
                .OrderBy(o => o.OrgId)
                .ToListAsync();

            return formatter.Format(
                "organisations",
                columns,
                organisations
            );
        }
    }
}
