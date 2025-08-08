using AutoMapper;
using crewbackend.Data;
using crewbackend.DTOs;
using crewbackend.Models;
using crewbackend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using CrewBackend.Exceptions.Domain;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using CrewBackend.Exceptions.Auth;

namespace crewbackend.Services
{
    public class OrganisationService : IOrganisationService
    {
        private readonly AppDbContext _appDbContext;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor; //to get the current user's ID from the authentication context

        public OrganisationService(
            AppDbContext appDbContext, 
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor)
        {
            _appDbContext = appDbContext;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                throw new AuthorizationException("User is not authenticated");
            }
            return userId;
        }

        public async Task<List<OrganisationResponseDTO>> GetAllOrganisationsAsync()
        {
            var organisations = await _appDbContext.Organisations
                .Include(o => o.OrganisationUsers)
                .ToListAsync();

            return organisations.Select(MapToResponseDTO).ToList();
        }

        public async Task<OrganisationResponseDTO?> GetOrganisationByIdAsync(int id)
        {
            var organisation = await _appDbContext.Organisations
                .Include(o => o.OrganisationUsers)
                    .ThenInclude(ou => ou.User)
                        .ThenInclude(u => u.Role)
                .FirstOrDefaultAsync(o => o.OrgId == id);

            if (organisation == null) return null;

            return MapToResponseDTO(organisation);
        }

        public async Task<OrganisationResponseDTO> CreateOrganisationAsync(OrganisationCreateDTO orgDto)
        {
            // Check if organisation with same name exists
            var existingOrg = await _appDbContext.Organisations
                .FirstOrDefaultAsync(o => o.OrgName.ToLower() == orgDto.OrgName.ToLower());

            if (existingOrg != null)
            {
                throw new ValidationException("org_name", "The organisation name has already been taken.");
            }

            var organisation = new Organisation
            {
                OrgName = orgDto.OrgName,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Add the organisation first to get its ID
            _appDbContext.Organisations.Add(organisation);
            await _appDbContext.SaveChangesAsync();

            // Add users to the organisation
            var currentUserId = GetCurrentUserId();
            foreach (var userId in orgDto.UserIds)
            {
                var user = await _appDbContext.Users.FindAsync(userId);
                if (user == null)
                {
                    throw new ValidationException("user_ids", $"User with ID {userId} not found.");
                }

                organisation.OrganisationUsers.Add(new OrganisationUser
                {
                    UserId = userId,
                    OrganisationId = organisation.OrgId,
                    AssignedBy = currentUserId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            await _appDbContext.SaveChangesAsync();

            // Reload the organisation with users to get the complete data
            var updatedOrganisation = await _appDbContext.Organisations
                .Include(o => o.OrganisationUsers)
                    .ThenInclude(ou => ou.User)
                        .ThenInclude(u => u.Role)
                .FirstAsync(o => o.OrgId == organisation.OrgId);

            return MapToResponseDTO(updatedOrganisation);
        }

        public async Task<bool> UpdateOrganisationAsync(int id, OrganisationUpdateDTO orgDto)
        {
            var organisation = await _appDbContext.Organisations
                .Include(o => o.OrganisationUsers)
                .FirstOrDefaultAsync(o => o.OrgId == id);

            if (organisation == null)
            {
                throw new EntityNotFoundException($"Organisation with ID {id} not found.");
            }

            // Check if new name conflicts with existing organisation
            var existingOrg = await _appDbContext.Organisations
                .FirstOrDefaultAsync(o => o.OrgName.ToLower() == orgDto.OrgName.ToLower() && o.OrgId != id);

            if (existingOrg != null)
            {
                throw new ValidationException("name", "The organisation name has already been taken.");
            }

            // Update basic info
            organisation.OrgName = orgDto.OrgName;
            organisation.UpdatedAt = DateTime.UtcNow;

            // Update user assignments
            // Remove users that are not in the new list
            var usersToRemove = organisation.OrganisationUsers
                .Where(ou => !orgDto.UserIds.Contains(ou.UserId))
                .ToList();

            foreach (var userToRemove in usersToRemove)
            {
                _appDbContext.Remove(userToRemove);
            }

            // Add new users
            var existingUserIds = organisation.OrganisationUsers.Select(ou => ou.UserId).ToList();
            var newUserIds = orgDto.UserIds.Except(existingUserIds);

            foreach (var userId in newUserIds)
            {
                var user = await _appDbContext.Users.FindAsync(userId);
                if (user == null)
                {
                    throw new ValidationException("user_ids", $"User with ID {userId} not found.");
                }

                var currentUserId = GetCurrentUserId();
                organisation.OrganisationUsers.Add(new OrganisationUser
                {
                    UserId = userId,
                    OrganisationId = id,
                    AssignedBy = currentUserId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            await _appDbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteOrganisationAsync(int id)
        {
            var organisation = await _appDbContext.Organisations
                .Include(o => o.OrganisationUsers)
                .FirstOrDefaultAsync(o => o.OrgId == id);

            if (organisation == null)
            {
                throw new EntityNotFoundException($"Organisation with ID {id} not found.");
            }

            if (organisation.OrganisationUsers.Any())
            {
                throw new ValidationException("general", "Cannot delete organisation that has users. Please remove all users first.");
            }

            _appDbContext.Organisations.Remove(organisation);
            await _appDbContext.SaveChangesAsync();
            return true;
        }

        public IQueryable<Organisation> QueryOrganisations()
        {
            return _appDbContext.Organisations.Include(o => o.OrganisationUsers);
        }

        private OrganisationResponseDTO MapToResponseDTO(Organisation org)
        {
            var users = org.OrganisationUsers
                .Select(ou => ou.User)
                .Select(user => new UserResponseDTO
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.Role.RoleName.ToUpper(),
                    Created_at = user.CreatedAt?.ToString("dd/MM/yyyy") ?? string.Empty
                })
                .ToList();

            return new OrganisationResponseDTO
            {
                OrgId = org.OrgId,
                OrgName = org.OrgName,
                CreatedAt = org.CreatedAt.ToString("dd/MM/yyyy"),
                UsersCount = org.OrganisationUsers.Count,
                Users = users
            };
        }
    }
}