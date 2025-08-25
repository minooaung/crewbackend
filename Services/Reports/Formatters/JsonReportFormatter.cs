using System.Text.Json;
using CrewBackend.Models;

namespace CrewBackend.Services.Reports.Formatters
{
    public class JsonReportFormatter : BaseReportFormatter
    {
        public override (byte[] Content, string ContentType) Format<T>(
            string reportType,
            Dictionary<string, string> columns,
            IEnumerable<T> data)
        {
            if (reportType != "organisations")
            {
                // For non-organisation reports, format dates specially
                var formattedData = PrepareData(columns, data).Select(row =>
                {
                    // Create an anonymous object with properly formatted properties
                    return new
                    {
                        Name = row["Name"]?.ToString(),
                        Email = row["Email"]?.ToString(),
                        Role = row["Role"]?.ToString(),
                        CreatedAt = DateTime.TryParse(row["Created At"]?.ToString(), out var date)
                            ? date.ToString("dd/MM/yyyy hh:mm:ss tt").ToLower()
                            : row["Created At"]?.ToString()
                    };
                });

                var defaultJson = JsonSerializer.Serialize(formattedData, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
                return (System.Text.Encoding.UTF8.GetBytes(defaultJson), "application/json");
            }

            // Special handling for organisations report
            var organisations = data as IEnumerable<Organisation>;
            if (organisations == null)
                throw new ArgumentException("Invalid data type for organisations report");

            var result = organisations.Select(org => new
            {
                Name = org.OrgName,
                CreatedAt = org.CreatedAt.ToString("dd/MM/yyyy hh:mm:ss tt").ToLower(),
                AssignedUsers = org.OrganisationUsers?.Any() == true 
                    ? org.OrganisationUsers
                        .Where(ou => ou.User != null && ou.User.Role != null)
                        .Select(ou => new
                        {
                            Name = ou.User.Name,
                            Email = ou.User.Email,
                            Role = ou.User.Role.RoleName,
                            CreatedAt = ou.User.CreatedAt?.ToString("dd/MM/yyyy hh:mm:ss tt").ToLower()
                        }).ToArray()
                    : new object[] { "No assigned users" }
            });

            var orgJson = JsonSerializer.Serialize(result, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            return (System.Text.Encoding.UTF8.GetBytes(orgJson), "application/json");
        }
    }
}
