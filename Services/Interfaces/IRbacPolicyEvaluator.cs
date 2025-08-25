using CrewBackend.Models;

namespace CrewBackend.Services.Interfaces
{
    /// <summary>
    /// Defines role-based access control evaluation logic for create, update, and delete operations.
    /// </summary>
    public interface IRbacPolicyEvaluator
    {
        bool CanCreate(User actor, string targetRoleName);
        bool CanUpdate(User actor, User target);
        bool CanDelete(User actor, User target);
    }
}
// This interface is used to evaluate whether a user has the necessary permissions
// to perform create, update, or delete operations on user roles within the system.