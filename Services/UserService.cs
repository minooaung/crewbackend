using AutoMapper;
using crewbackend.Data;
using crewbackend.DTOs;
using crewbackend.Models;
using crewbackend.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using crewbackend.Helpers;
using CrewBackend.Exceptions.Domain;

namespace crewbackend.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _appDbContext;
        private readonly IMapper _mapper;
        private readonly IPasswordHasher<User> _passwordHasher;

        public UserService(AppDbContext appDbContext, IMapper mapper, IPasswordHasher<User> passwordHasher) {
            _appDbContext = appDbContext;
            _mapper = mapper;
            _passwordHasher = passwordHasher;
        }        

        public async Task<List<UserResponseDTO>> GetAllUsersAsync()
        {
            if (_appDbContext.Users == null) return new List<UserResponseDTO>();

            var users = await _appDbContext.Users.ToListAsync();
            
            return users.Select(UserResponseMapper.MapToUserResponseDTO).ToList();
        }
        
        public async Task<UserResponseDTO?> GetUserByIdAsync(int id)
        {
            if (_appDbContext.Users == null) return null;

            var user = await _appDbContext.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return null;

            return UserResponseMapper.MapToUserResponseDTO(user);
        }

        public async Task<UserResponseDTO> CreateUserAsync(UserCreateDTO userDto)
        {            
            // Check if the email exists among non-deleted users
            var existingUser = await _appDbContext.Users
                                    .FirstOrDefaultAsync(u => u.Email == userDto.Email && !u.IsDeleted);

            if (existingUser != null)
            {
                throw new ValidationException("email", "The email has already been taken.");
            }

            // Check if there's a soft-deleted user with this email
            var deletedUser = await _appDbContext.Users
                                    .Include(u => u.OrganisationUsers)
                                    .FirstOrDefaultAsync(u => u.Email == userDto.Email && u.IsDeleted);

            if (deletedUser != null)
            {
                // First, hard delete all associated OrganisationUsers records
                if (deletedUser.OrganisationUsers != null && deletedUser.OrganisationUsers.Any())
                {
                    _appDbContext.OrganisationUsers.RemoveRange(deletedUser.OrganisationUsers);
                }

                // Then hard delete the user record to allow reuse of the email
                _appDbContext.Users.Remove(deletedUser);
                await _appDbContext.SaveChangesAsync();
            }

            if (!string.IsNullOrWhiteSpace(userDto.Password))
            {
                if (userDto.Password != userDto.Password_Confirmation)
                {                    
                    throw new ValidationException("password_confirmation", "The password confirmation does not match.");
                }                
            }

            // Get role - if no role specified, default to Employee
            var role = string.IsNullOrEmpty(userDto.Role)
                ? await _appDbContext.UserRoles.FirstOrDefaultAsync(r => r.RoleName.ToUpper() == "EMPLOYEE")
                : await _appDbContext.UserRoles.FirstOrDefaultAsync(r => r.RoleName.ToUpper() == userDto.Role.ToUpper());

            if (role == null)
            {
                throw new ValidationException("role", $"Unable to assign role. Please contact administrator.");
            }

            // Mapping logic stays in UserProfile.cs
            // AutoMapper maps UserCreateDTO to User entity without Role as role is set separately
            var user = _mapper.Map<User>(userDto);
            user.RoleId = role.RoleId;

            // Hash the password using the password hasher
            user.Password = _passwordHasher.HashPassword(user, user.Password);

            // Set CreatedAt and UpdatedAt timestamps
            user.CreatedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;

            // Add the user to the database
            _appDbContext.Users.Add(user);
            await _appDbContext.SaveChangesAsync();

            // Map the user to a UserResponseMapper to return the user with the role
            return UserResponseMapper.MapToUserResponseDTO(user);
        }

        public async Task<UserResponseDTO> UpdateUserAsync(int id, UserUpdateDTO userDto)
        {
            var existingUser = await _appDbContext.Users
                .Include(u => u.Role)  // Include Role to ensure it's available for mapping
                .FirstOrDefaultAsync(u => u.Id == id);
                
            if (existingUser == null) 
            {
                throw new EntityNotFoundException($"User with ID {id} not found.");
            }
            
            // Handle password update first
            if (!string.IsNullOrWhiteSpace(userDto.Password))
            {
                if (userDto.Password != userDto.Password_Confirmation)
                {
                    throw new ValidationException("password_confirmation", "The password confirmation does not match.");
                }
                
                // Only hash and update password if it was provided
                existingUser.Password = _passwordHasher.HashPassword(existingUser, userDto.Password);
            }

            // Mapping logic stays in UserProfile.cs
            // AutoMapper maps UserUpdateDTO to User entity
            _mapper.Map(userDto, existingUser);

            // Set timestamp after mapping
            existingUser.UpdatedAt = DateTime.UtcNow;

            await _appDbContext.SaveChangesAsync();

            // Map (User -> UserResponseDTO) to a UserResponseMapper to return the user with the role
            return UserResponseMapper.MapToUserResponseDTO(existingUser);
        }

        public async Task<bool> DeleteUserAsync(int id, int deletedByUserId)
        {
            var user = await _appDbContext.Users
                .Include(u => u.OrganisationUsers)
                .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);
                
            if (user == null) 
            {
                throw new EntityNotFoundException($"User with ID {id} not found.");
            }

            // Perform soft delete on user
            user.IsDeleted = true;
            user.DeletedAt = DateTime.UtcNow;
            user.DeletedByUserId = deletedByUserId;

            // Soft delete all OrganisationUser records
            if (user.OrganisationUsers != null)
            {
                foreach (var orgUser in user.OrganisationUsers)
                {
                    orgUser.IsDeleted = true;
                    orgUser.DeletedAt = DateTime.UtcNow;
                    orgUser.DeletedByUserId = deletedByUserId;
                }
            }

            await _appDbContext.SaveChangesAsync();
            return true;
        }
        
        public async Task<User?> AuthenticateAsync(string email, string password)
        {
            if (_appDbContext.Users == null) return null;

            var user = await _appDbContext.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted);

            if (user == null) return null;

            var result = _passwordHasher.VerifyHashedPassword(user, user.Password, password);
            if (result != PasswordVerificationResult.Success)
            {
                return null;
            }

            return user;
        }

        // Get user by email only
        public async Task<UserResponseDTO?> GetUserByEmailAsync(string email)
        {
            var user = await _appDbContext.Users
                .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted);

            return user == null ? null : UserResponseMapper.MapToUserResponseDTO(user);
        }        

        public IQueryable<User> QueryUsers()
        {
            return _appDbContext.Users
                .Include(u => u.Role)
                .Where(u => !u.IsDeleted)
                .AsQueryable();
        }
    }
}