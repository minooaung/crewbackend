namespace CrewBackend.Services.Reports
{
    public interface IReportService
    {
        Task<(byte[] Content, string ContentType)> GenerateAsync(string reportType, string outputFormat);
    }
}
