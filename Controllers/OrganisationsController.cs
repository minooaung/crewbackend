using crewbackend.DTOs;
using crewbackend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using crewbackend.Helpers;
using CrewBackend.Exceptions.Domain;

namespace crewbackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrganisationsController : ControllerBase
    {
        private readonly IOrganisationService _organisationService;

        public OrganisationsController(IOrganisationService organisationService)
        {
            _organisationService = organisationService;
        }

        [HttpGet]
        public async Task<ActionResult> GetOrganisations(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null)
        {
            var query = _organisationService.QueryOrganisations();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(o => o.OrgName.Contains(search));
            }

            var total = await query.CountAsync();
            var organisations = await query
                .OrderBy(o => o.OrgId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var formattedOrganisations = organisations
                .Select(org => new OrganisationResponseDTO
                {
                    OrgId = org.OrgId,
                    OrgName = org.OrgName,
                    CreatedAt = org.CreatedAt.ToString("dd/MM/yyyy"),
                    UsersCount = org.OrganisationUsers.Count
                })
                .ToList();

            // Pagination calculations
            var lastPage = (int)Math.Ceiling(total / (double)pageSize);
            var from = ((page - 1) * pageSize) + 1;
            var to = Math.Min(page * pageSize, total);

            // Prepare pagination links
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            
            var pageLinks = Enumerable.Range(1, lastPage)
                .Select(p => new PaginationLink
                {
                    Url = $"{baseUrl}?page={p}",
                    Label = p.ToString(),
                    Active = p == page
                }).ToList();

            pageLinks.Insert(0, new PaginationLink
            {
                Url = page > 1 ? $"{baseUrl}?page={page - 1}" : null,
                Label = "&laquo; Previous",
                Active = false
            });

            pageLinks.Add(new PaginationLink
            {
                Url = page < lastPage ? $"{baseUrl}?page={page + 1}" : null,
                Label = "Next &raquo;",
                Active = false
            });

            var links = new
            {
                first = $"{baseUrl}?page=1",
                last = $"{baseUrl}?page={lastPage}",
                prev = page > 1 ? $"{baseUrl}?page={page - 1}" : null,
                next = page < lastPage ? $"{baseUrl}?page={page + 1}" : null
            };

            var meta = new
            {
                current_page = page,
                from = from,
                last_page = lastPage,
                links = pageLinks,
                path = baseUrl,
                per_page = pageSize,
                to = to,
                total = total
            };

            return Ok(new
            {
                data = formattedOrganisations,
                links,
                meta
            });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OrganisationResponseDTO>> GetOrganisationById(int id)
        {
            var organisation = await _organisationService.GetOrganisationByIdAsync(id);

            if (organisation == null)
            {
                throw new EntityNotFoundException($"Organisation with ID {id} not found.");
            }

            return Ok(organisation);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateOrganisation([FromBody] OrganisationCreateDTO orgDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key.ToLower(),
                        kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? new[] { "Invalid value" }
                    );
                throw new ValidationException(errors);
            }

            var createdOrg = await _organisationService.CreateOrganisationAsync(orgDto);
            return CreatedAtAction(nameof(GetOrganisationById), new { id = createdOrg.OrgId }, createdOrg);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrganisation(int id, [FromBody] OrganisationUpdateDTO orgDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key.ToLower(),
                        kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? new[] { "Invalid value" }
                    );
                throw new ValidationException(errors);
            }

            await _organisationService.UpdateOrganisationAsync(id, orgDto);
            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrganisation(int id)
        {
            await _organisationService.DeleteOrganisationAsync(id);
            return NoContent();
        }
    }
}