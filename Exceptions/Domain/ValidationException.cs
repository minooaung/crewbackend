using System.Collections.Generic;
using System.Net;
using CrewBackend.Exceptions.Base;

namespace CrewBackend.Exceptions.Domain
{
    public class ValidationException : AppException
    {
        public Dictionary<string, string[]> Details { get; }

        public ValidationException(Dictionary<string, string[]> details) 
            : base("Invalid input.", HttpStatusCode.UnprocessableEntity)
        {
            Details = details;
        }

        public ValidationException(string field, string error) 
            : base("Invalid input.", HttpStatusCode.UnprocessableEntity)
        {
            Details = new Dictionary<string, string[]>
            {
                { field.ToLower(), new[] { error } }
            };
        }
    }
}