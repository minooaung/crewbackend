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
            
            return _mapper.Map<List<UserResponseDTO>>(users);
        }
        public async Task<UserResponseDTO?> GetUserByIdAsync(int id)
        {
            if (_appDbContext.Users == null) return null;

            var user = await _appDbContext.Users.FindAsync(id);
            if (user == null) return null;

            return _mapper.Map<UserResponseDTO>(user);            
        }

        public async Task<UserResponseDTO> CreateUserAsync(UserCreateDTO userDto)
        {            
            // Check if the email already exists
            var existingUser = await _appDbContext.Users
                                    .FirstOrDefaultAsync(u => u.Email == userDto.Email);

            if (existingUser != null)
            {
                throw new ValidationException("email", "The email has already been taken.");
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

            // Map UserCreateDTO to User entity
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

            return UserResponseMapper.MapToUserResponseDTO(user);
        }

        public async Task<bool> UpdateUserAsync(int id, UserUpdateDTO userDto)
        {
            var existingUser = await _appDbContext.Users.FindAsync(id);
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

                existingUser.Password = _passwordHasher.HashPassword(existingUser, userDto.Password);
            }

            existingUser.UpdatedAt = DateTime.UtcNow;

            // Now map the rest of the properties (excluding Password)
            _mapper.Map(userDto, existingUser);            

            await _appDbContext.SaveChangesAsync();
            return true;
        }
        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _appDbContext.Users.FindAsync(id);
            if (user == null) 
            {
                throw new EntityNotFoundException($"User with ID {id} not found.");
            }

            _appDbContext.Users.Remove(user);
            await _appDbContext.SaveChangesAsync();
            return true;
        }
        
        public async Task<User?> AuthenticateAsync(string email, string password)
        {
            if (_appDbContext.Users == null) return null;

            var user = await _appDbContext.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == email);

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
                .FirstOrDefaultAsync(u => u.Email == email);

            return user == null ? null : _mapper.Map<UserResponseDTO>(user);
        }        

        public IQueryable<User> QueryUsers()
        {
            return _appDbContext.Users.Include(u => u.Role).AsQueryable();
        }
    }
}