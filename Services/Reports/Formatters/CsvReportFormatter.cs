using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

namespace CrewBackend.Services.Reports.Formatters
{
    public class CsvReportFormatter : BaseReportFormatter
    {
        public override (byte[] Content, string ContentType) Format<T>(
            string reportType,
            Dictionary<string, string> columns,
            IEnumerable<T> data)
        {
            using var memoryStream = new MemoryStream();
            using var writer = new StreamWriter(memoryStream);
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                ShouldQuote = args => true, // Always quote fields to handle multiline content
                NewLine = "\r\n" // Ensure consistent line endings
            };
            using var csv = new CsvWriter(writer, config);

            var formattedData = PrepareData(columns, data);

            // Write headers
            foreach (var column in columns.Keys)
            {
                csv.WriteField(column);
            }
            csv.NextRecord();

            // Write data
            foreach (var row in formattedData)
            {
                foreach (var column in columns.Keys)
                {
                    var value = row[column]?.ToString() ?? string.Empty;
                    // Replace \n with literal \r\n for Excel compatibility
                    value = value.Replace("\n", "\r\n");
                    csv.WriteField(value);
                }
                csv.NextRecord();
            }

            writer.Flush();
            return (memoryStream.ToArray(), "text/csv");
        }
    }
}
