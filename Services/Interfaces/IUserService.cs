using crewbackend.Models;
using crewbackend.DTOs;
using crewbackend.Services.Interfaces;

namespace crewbackend.Services.Interfaces
{
    public interface IUserService
    {
        // Task<List<User>> GetAllUsersAsync();
        // Task<User?> GetUserByIdAsync(int id);
        // Task<User> CreateUserAsync(User user);
        // Task<bool> UpdateUserAsync(int id, User updatedUser);
        // Task<bool> DeleteUserAsync(int id);

        Task<List<UserResponseDTO>> GetAllUsersAsync();
        Task<UserResponseDTO?> GetUserByIdAsync(int id);
        Task<UserResponseDTO> CreateUserAsync(UserCreateDTO userDto);
        Task<bool> UpdateUserAsync(int id, UserUpdateDTO userDto);
        Task<bool> DeleteUserAsync(int id);

        //Task<UserResponseDTO?> AuthenticateAsync(string email, string password);
        Task<User?> AuthenticateAsync(string email, string password);
        Task<UserResponseDTO?> GetUserByEmailAsync(string email);

        IQueryable<User> QueryUsers(); // Added QueryUsers method
    }
}
