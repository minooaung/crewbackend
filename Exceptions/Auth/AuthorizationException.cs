using System.Net;
using CrewBackend.Exceptions.Base;

namespace CrewBackend.Exceptions.Auth
{
    public class AuthorizationException : AppException
    {
        public AuthorizationException(string message = "Unauthorized action. You do not have permission to perform this request.") 
            : base(message, HttpStatusCode.Forbidden)
        {
        }
    }
}