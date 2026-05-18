using System.Globalization;
using CrewBackend.DTOs;
using CrewBackend.Models;

namespace CrewBackend.Helpers
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
                Role = user.Role?.RoleName?.ToUpper() ?? string.Empty,
                Created_at = user.CreatedAt?.ToString("dd/MM/yyyy") ?? string.Empty
            };
        }
    }
}