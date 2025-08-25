using CrewBackend.Models;
using CrewBackend.Services.Interfaces;

namespace CrewBackend.Services
{
    /// <summary>
    /// Centralized evaluator for RBAC policies based on role names and user relationships.
    /// </summary>
    public class RbacPolicyEvaluator : IRbacPolicyEvaluator
    {
        public bool CanCreate(User actor, string targetRoleName)
        {
            var actorRole = actor.Role.RoleName;

            return actorRole switch
            {
                UserRoleConstants.SuperAdmin => true,
                UserRoleConstants.Admin => targetRoleName == UserRoleConstants.Employee,
                _ => false
            };
        }

        public bool CanUpdate(User actor, User target)
        {
            var actorRole = actor.Role.RoleName;
            var targetRole = target.Role.RoleName;
            var isSelf = actor.Id == target.Id;

            if (isSelf) return true;

            return actorRole switch
            {
                UserRoleConstants.SuperAdmin => true,
                UserRoleConstants.Admin => targetRole == UserRoleConstants.Employee,
                _ => false
            };
        }

        public bool CanDelete(User actor, User target)
        {
            var actorRole = actor.Role.RoleName;
            var targetRole = target.Role.RoleName;
            var isSelf = actor.Id == target.Id;

            if (isSelf) return false;

            return actorRole switch
            {
                UserRoleConstants.SuperAdmin => true,
                UserRoleConstants.Admin => targetRole == UserRoleConstants.Employee,
                _ => false
            };
        }
    }
}
