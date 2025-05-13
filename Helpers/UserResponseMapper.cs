using System.Globalization;
using crewbackend.DTOs;
using crewbackend.Models;

namespace crewbackend.Helpers
{
    public static class UserResponseMapper
    {
        public static UserResponseDTO MapToUserResponseDTO(User user)
        {
            if (user == null) return null!;

            return new UserResponseDTO
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Created_at = user.CreatedAt?.ToString("dd/MM/yyyy"),
                Updated_at = user.UpdatedAt?.ToString("dd/MM/yyyy")
            };
        }
    }
}