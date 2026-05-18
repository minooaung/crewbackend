using System.Net;
using CrewBackend.Exceptions.Base;

namespace CrewBackend.Exceptions.Auth
{
    public class AuthenticationException : AppException
    {
        public AuthenticationException(string message = "Unauthenticated. Please log in again.") 
            : base(message, HttpStatusCode.Unauthorized)
        {
        }
    }
}