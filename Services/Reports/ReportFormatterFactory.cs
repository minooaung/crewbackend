using CrewBackend.Services.Reports.Formatters;

namespace CrewBackend.Services.Reports
{
    public class ReportFormatterFactory
    {
        private readonly Dictionary<string, IReportFormatter> _formatters;

        public ReportFormatterFactory()
        {
            _formatters = new Dictionary<string, IReportFormatter>
            {
                ["pdf"] = new PdfReportFormatter(),
                ["excel"] = new ExcelReportFormatter(),
                ["csv"] = new CsvReportFormatter(),
                ["json"] = new JsonReportFormatter(),
                ["html"] = new HtmlReportFormatter()
            };
        }

        public IReportFormatter GetFormatter(string format)
        {
            if (!_formatters.TryGetValue(format.ToLower(), out var formatter))
            {
                throw new ArgumentException($"Unsupported format: {format}");
            }

            return formatter;
        }
    }
}
