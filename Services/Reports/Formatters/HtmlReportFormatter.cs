using System.Text;

namespace CrewBackend.Services.Reports.Formatters
{
    public class HtmlReportFormatter : BaseReportFormatter
    {
        public override (byte[] Content, string ContentType) Format<T>(
            string reportType,
            Dictionary<string, string> columns,
            IEnumerable<T> data)
        {
            var html = new StringBuilder();
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html>");
            html.AppendLine("<head>");
            html.AppendLine("<style>");
            html.AppendLine(@"
                table { border-collapse: collapse; width: 100%; }
                th, td { 
                    border: 1px solid #ddd; 
                    padding: 12px; 
                    text-align: left;
                    vertical-align: top;
                    line-height: 1.5;
                }
                th { background-color: #f2f2f2; }
                tr:nth-child(even) { background-color: #f9f9f9; }
                tr:hover { background-color: #f5f5f5; }
            ");
            html.AppendLine("</style>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            
            html.AppendLine($"<h2>{char.ToUpper(reportType[0]) + reportType[1..]} Report</h2>");
            html.AppendLine("<table>");

            // Headers
            html.AppendLine("<tr>");
            foreach (var column in columns.Keys)
            {
                html.AppendLine($"<th>{column}</th>");
            }
            html.AppendLine("</tr>");

            // Data
            var formattedData = PrepareData(columns, data);
            foreach (var row in formattedData)
            {
                html.AppendLine("<tr>");
                foreach (var column in columns.Keys)
                {
                    var value = row[column]?.ToString() ?? string.Empty;
                    // Replace \n with <br/> for HTML line breaks
                    value = value.Replace("\n", "<br/>");
                    html.AppendLine($"<td>{value}</td>");
                }
                html.AppendLine("</tr>");
            }

            html.AppendLine("</table>");
            html.AppendLine("</body>");
            html.AppendLine("</html>");

            return (Encoding.UTF8.GetBytes(html.ToString()), "text/html");
        }
    }
}
