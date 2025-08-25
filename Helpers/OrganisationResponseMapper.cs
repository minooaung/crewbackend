using CrewBackend.DTOs;
using CrewBackend.Models;

namespace CrewBackend.Helpers
{
    public static class OrganisationResponseMapper
    {
        public static OrganisationResponseDTO MapToOrganisationResponseDTO(Organisation org)
        {
            if (org == null) return null!;

            var activeUsers = org.OrganisationUsers?
                .Where(ou => !ou.IsDeleted && ou.User != null && !ou.User.IsDeleted)
                .Select(ou => ou.User)
                .ToList() ?? new List<User>();

            var userDTOs = activeUsers
                .Select(user => UserResponseMapper.MapToUserResponseDTO(user))
                .ToList();

            return new OrganisationResponseDTO
            {
                OrgId = org.OrgId,
                OrgName = org.OrgName,
                CreatedAt = org.CreatedAt.ToString("dd/MM/yyyy"),
                UsersCount = activeUsers.Count,
                Users = userDTOs
            };
        }
    }
}
