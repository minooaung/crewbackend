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
using crewbackend.Helpers;

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

            return organisations.Select(OrganisationResponseMapper.MapToOrganisationResponseDTO).ToList();
        }

        public async Task<OrganisationResponseDTO?> GetOrganisationByIdAsync(int id)
        {
            var organisation = await _appDbContext.Organisations
                .Include(o => o.OrganisationUsers)
                    .ThenInclude(ou => ou.User)
                        .ThenInclude(u => u.Role)
                .FirstOrDefaultAsync(o => o.OrgId == id);

            if (organisation == null) return null;

            return OrganisationResponseMapper.MapToOrganisationResponseDTO(organisation);
        }

        public async Task<OrganisationResponseDTO> CreateOrganisationAsync(OrganisationCreateDTO orgDto)
        {
            // Check if the name exists among non-deleted organizations
            var existingOrg = await _appDbContext.Organisations
                .FirstOrDefaultAsync(o => o.OrgName.ToLower() == orgDto.OrgName.ToLower() && !o.IsDeleted);

            if (existingOrg != null)
            {
                throw new ValidationException("org_name", "The organisation name has already been taken.");
            }

            // Check if there's a soft-deleted organization with this name
            var deletedOrg = await _appDbContext.Organisations
                .Include(o => o.OrganisationUsers) // Includes OrganisationUsers to handle associated records
                .FirstOrDefaultAsync(o => o.OrgName.ToLower() == orgDto.OrgName.ToLower() && o.IsDeleted);

            if (deletedOrg != null)
            {
                // If found, hard delete the old organization and its associations from OrganisationUsers
                // This is to prevent the new organization from being associated with the soft-deleted organization's users
                if (deletedOrg.OrganisationUsers != null && deletedOrg.OrganisationUsers.Any())
                {
                    _appDbContext.OrganisationUsers.RemoveRange(deletedOrg.OrganisationUsers);
                }
                _appDbContext.Organisations.Remove(deletedOrg);
                await _appDbContext.SaveChangesAsync();
            }

            // Mapping logic stays in OrganisationProfile.cs
            // AutoMapper maps OrganisationCreateDTO to Organisation entity
            var organisation = _mapper.Map<Organisation>(orgDto);
            organisation.CreatedAt = DateTime.UtcNow;
            organisation.UpdatedAt = DateTime.UtcNow;

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

            return OrganisationResponseMapper.MapToOrganisationResponseDTO(updatedOrganisation);
        }

        public async Task<OrganisationResponseDTO> UpdateOrganisationAsync(int id, OrganisationUpdateDTO orgDto)
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

            // Mapping logic stays in OrganisationProfile.cs
            // AutoMapper maps OrganisationUpdateDTO to Organisation entity
            _mapper.Map(orgDto, organisation);            

            // Set timestamp after mapping
            organisation.UpdatedAt = DateTime.UtcNow;

            // Update user assignments
            // Soft delete users that are not in the new list
            var currentUserId = GetCurrentUserId();
            var usersToRemove = organisation.OrganisationUsers
                .Where(ou => !orgDto.UserIds.Contains(ou.UserId) && !ou.IsDeleted)
                .ToList();

            foreach (var userToRemove in usersToRemove)
            {
                userToRemove.IsDeleted = true;
                userToRemove.DeletedAt = DateTime.UtcNow;
                userToRemove.DeletedByUserId = currentUserId;
            }

            // Add new users or reactivate soft-deleted ones
            var activeUserIds = organisation.OrganisationUsers
                .Where(ou => !ou.IsDeleted)
                .Select(ou => ou.UserId)
                .ToList();
            var newUserIds = orgDto.UserIds.Except(activeUserIds);

            // Reactivate any soft-deleted associations
            var softDeletedAssociations = organisation.OrganisationUsers
                .Where(ou => ou.IsDeleted && orgDto.UserIds.Contains(ou.UserId))
                .ToList();

            foreach (var association in softDeletedAssociations)
            {
                association.IsDeleted = false;
                association.DeletedAt = null;
                association.DeletedByUserId = null;
                association.UpdatedAt = DateTime.UtcNow;
                newUserIds = newUserIds.Where(id => id != association.UserId);
            }

            foreach (var userId in newUserIds)
            {
                var user = await _appDbContext.Users.FindAsync(userId);
                if (user == null)
                {
                    throw new ValidationException("user_ids", $"User with ID {userId} not found.");
                }

                organisation.OrganisationUsers.Add(new OrganisationUser
                {
                    UserId = userId,
                    OrganisationId = id,
                    AssignedBy = currentUserId, // Using the currentUserId from outer scope
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
                .FirstAsync(o => o.OrgId == id);

            return OrganisationResponseMapper.MapToOrganisationResponseDTO(updatedOrganisation);
        }

        public async Task<bool> DeleteOrganisationAsync(int id)
        {
            var organisation = await _appDbContext.Organisations
                .Include(o => o.OrganisationUsers)
                .FirstOrDefaultAsync(o => o.OrgId == id && !o.IsDeleted);

            if (organisation == null)
            {
                throw new EntityNotFoundException($"Organisation with ID {id} not found.");
            }

            var currentUserId = GetCurrentUserId();
            var currentTime = DateTime.UtcNow;

            // Soft delete the organisation
            organisation.IsDeleted = true;
            organisation.DeletedAt = currentTime;
            organisation.DeletedByUserId = currentUserId;

            // Soft delete all associated OrganisationUser records
            foreach (var orgUser in organisation.OrganisationUsers.Where(ou => !ou.IsDeleted))
            {
                orgUser.IsDeleted = true;
                orgUser.DeletedAt = currentTime;
                orgUser.DeletedByUserId = currentUserId;
            }

            await _appDbContext.SaveChangesAsync();
            return true;
        }

        public IQueryable<Organisation> QueryOrganisations()
        {
            return _appDbContext.Organisations
                .Where(o => !o.IsDeleted)
                .Include(o => o.OrganisationUsers)
                    .ThenInclude(ou => ou.User);
        }


    }
}