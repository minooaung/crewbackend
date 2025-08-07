using System;
using System.Net;

namespace CrewBackend.Exceptions.Base
{
    public abstract class AppException : Exception
    {
        public HttpStatusCode StatusCode { get; }
        public string ErrorMessage { get; }

        protected AppException(string message, HttpStatusCode statusCode = HttpStatusCode.InternalServerError) 
            : base(message)
        {
            StatusCode = statusCode;
            ErrorMessage = message;
        }
    }
}