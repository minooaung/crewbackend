using AutoMapper;
using crewbackend.Data;
using crewbackend.DTOs;
using crewbackend.Models;
using crewbackend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using CrewBackend.Exceptions.Domain;

namespace crewbackend.Services
{
    public class OrganisationService : IOrganisationService
    {
        private readonly AppDbContext _appDbContext;
        private readonly IMapper _mapper;

        public OrganisationService(AppDbContext appDbContext, IMapper mapper)
        {
            _appDbContext = appDbContext;
            _mapper = mapper;
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

            _appDbContext.Organisations.Add(organisation);
            await _appDbContext.SaveChangesAsync();

            return MapToResponseDTO(organisation);
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
                throw new ValidationException("org_name", "The organisation name has already been taken.");
            }

            organisation.OrgName = orgDto.OrgName;
            organisation.UpdatedAt = DateTime.UtcNow;

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

        private static OrganisationResponseDTO MapToResponseDTO(Organisation org)
        {
            return new OrganisationResponseDTO
            {
                OrgId = org.OrgId,
                OrgName = org.OrgName,
                CreatedAt = org.CreatedAt.ToString("dd/MM/yyyy"),
                UsersCount = org.OrganisationUsers.Count
            };
        }
    }
}