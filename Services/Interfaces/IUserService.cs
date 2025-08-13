using crewbackend.Models;
using crewbackend.DTOs;
using crewbackend.Services.Interfaces;

namespace crewbackend.Services.Interfaces
{
    public interface IUserService
    {
        Task<List<UserResponseDTO>> GetAllUsersAsync();
        Task<UserResponseDTO?> GetUserByIdAsync(int id);
        Task<UserResponseDTO> CreateUserAsync(UserCreateDTO userDto);
        Task<UserResponseDTO> UpdateUserAsync(int id, UserUpdateDTO userDto);
        Task<bool> DeleteUserAsync(int id, int deletedByUserId);

        //Task<UserResponseDTO?> AuthenticateAsync(string email, string password);
        Task<User?> AuthenticateAsync(string email, string password);
        Task<UserResponseDTO?> GetUserByEmailAsync(string email);

        IQueryable<User> QueryUsers(); // Added QueryUsers method
    }
}
