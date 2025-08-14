namespace CrewBackend.Services.Reports
{
    public static class ReportMetadata
    {
        public static (string Extension, string ContentType) GetForFormat(string format) => format switch
        {
            "pdf" => ("pdf", "application/pdf"),
            "excel" => ("xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"),
            "csv" => ("csv", "text/csv"),
            "json" => ("json", "application/json"),
            "html" => ("html", "text/html"),
            _ => throw new ArgumentException($"Unsupported format: {format}")
        };
    }
}
