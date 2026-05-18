using System.Net;
using CrewBackend.Exceptions.Base;

namespace CrewBackend.Exceptions.Domain
{
    public class EntityNotFoundException : AppException
    {
        public EntityNotFoundException(string message = "Resource not found.") 
            : base(message, HttpStatusCode.NotFound)
        {
        }
    }
}