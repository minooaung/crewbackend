using CrewBackend.DTOs;
using CrewBackend.Models;

namespace CrewBackend.Services.Interfaces
{
    public interface IOrganisationService
    {
        Task<List<OrganisationResponseDTO>> GetAllOrganisationsAsync();
        Task<OrganisationResponseDTO?> GetOrganisationByIdAsync(int id);
        Task<OrganisationResponseDTO> CreateOrganisationAsync(OrganisationCreateDTO orgDto);
        Task<OrganisationResponseDTO> UpdateOrganisationAsync(int id, OrganisationUpdateDTO orgDto);
        Task<bool> DeleteOrganisationAsync(int id);
        IQueryable<Organisation> QueryOrganisations();
    }
}