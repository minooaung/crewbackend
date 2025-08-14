using System.Collections.Generic;

namespace CrewBackend.Services.Reports
{
    public interface IReportFormatter
    {
        (byte[] Content, string ContentType) Format<T>(string reportType, Dictionary<string, string> columns, IEnumerable<T> data);
    }
}
