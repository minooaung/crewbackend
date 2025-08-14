using ClosedXML.Excel;

namespace CrewBackend.Services.Reports.Formatters
{
    public class ExcelReportFormatter : BaseReportFormatter
    {
        public override (byte[] Content, string ContentType) Format<T>(
            string reportType,
            Dictionary<string, string> columns,
            IEnumerable<T> data)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Report");

            // Write headers
            var headerRow = worksheet.Row(1);
            var columnIndex = 1;
            foreach (var column in columns.Keys)
            {
                headerRow.Cell(columnIndex++).Value = column;
            }

            // Write data
            var formattedData = PrepareData(columns, data);
            var rowIndex = 2;
            foreach (var row in formattedData)
            {
                columnIndex = 1;
                foreach (var column in columns.Keys)
                {
                    worksheet.Cell(rowIndex, columnIndex++).Value = row[column]?.ToString() ?? string.Empty;
                }
                rowIndex++;
            }

            // Auto-fit columns
            worksheet.Columns().AdjustToContents();

            using var memoryStream = new MemoryStream();
            workbook.SaveAs(memoryStream);
            return (memoryStream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        }
    }
}
