using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Geom;
using iText.Kernel.Colors;

namespace CrewBackend.Services.Reports.Formatters
{
    public class PdfReportFormatter : BaseReportFormatter
    {
        // No static initialization needed for basic PDF generation
        public override (byte[] Content, string ContentType) Format<T>(
            string reportType,
            Dictionary<string, string> columns,
            IEnumerable<T> data)
        {
            using var memoryStream = new MemoryStream();
            var writer = new PdfWriter(memoryStream);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf, PageSize.A4);

            // Add title
            var title = new Paragraph($"{char.ToUpper(reportType[0]) + reportType[1..]} Report")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(16);
            document.Add(title);
            document.Add(new Paragraph("\n"));

            try
            {
                // Create table
                var table = new Table(columns.Count);
                table.SetWidth(UnitValue.CreatePercentValue(100));

                // Add headers
                foreach (var column in columns.Keys)
                {
                    var cell = new Cell()
                        .Add(new Paragraph(column))
                        .SetBackgroundColor(iText.Kernel.Colors.ColorConstants.LIGHT_GRAY)
                        .SetBold();
                    table.AddCell(cell);
                }

                // Add data
                var formattedData = PrepareData(columns, data);
                foreach (var row in formattedData)
                {
                    foreach (var column in columns.Keys)
                    {
                        var value = row[column]?.ToString() ?? string.Empty;
                        // Handle multiline content by replacing \n with actual line breaks
                        var lines = value.Split('\n');
                        var cell = new Cell();
                        for (var i = 0; i < lines.Length; i++)
                        {
                            cell.Add(new Paragraph(lines[i]));
                            if (i < lines.Length - 1)
                            {
                                // Add a smaller line spacing between items
                                cell.Add(new Paragraph(" ").SetFixedLeading(5));
                            }
                        }
                        table.AddCell(cell);
                    }
                }

                document.Add(table);
                document.Close();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error generating PDF: {ex.Message}", ex);
            }

            return (memoryStream.ToArray(), "application/pdf");
        }
    }
}
