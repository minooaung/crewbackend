using System.Collections.Generic;

namespace CrewBackend.Models.Responses
{
    public class ErrorResponse
    {
        public required string Error { get; set; }
        public Dictionary<string, string[]>? Details { get; set; }
    }
}