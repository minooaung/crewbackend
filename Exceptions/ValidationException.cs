using System;
using System.Collections.Generic;

namespace crewbackend.Exceptions
{
    public class ValidationException : Exception
    {
        public IDictionary<string, string[]> Errors { get; }

        public ValidationException(string message, string field)
            : base(message)
        {
            Errors = new Dictionary<string, string[]>
            {
                { field, new[] { message } }
            };
        }

        public ValidationException(IDictionary<string, string[]> errors)
            : base("One or more validation errors occurred.")
        {
            Errors = errors;
        }
    }
}
