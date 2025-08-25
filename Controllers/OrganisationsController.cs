using CrewBackend.DTOs;
using CrewBackend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using CrewBackend.Helpers;
using CrewBackend.Exceptions.Domain;
using CrewBackend.Exceptions.Auth;
using CrewBackend.Models;
using System.Security.Claims;

namespace CrewBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrganisationsController : ControllerBase
    {
        private readonly IOrganisationService _organisationService;
        private readonly IRbacPolicyEvaluator _rbac;
        private readonly IUserService _userService;

        public OrganisationsController(IOrganisationService organisationService, IRbacPolicyEvaluator rbac, IUserService userService)
        {
            _organisationService = organisationService;
            _rbac = rbac;
            _userService = userService;
        }

        /// <summary>
        /// Helper method to get the current authenticated user from JWT claims
        /// </summary>
        private async Task<User> GetCurrentUserAsync()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int currentUserId))
            {
                throw new AuthorizationException("User ID not found in claims or invalid");
            }

            // QueryUsers already includes Role and filters deleted users
            var currentUser = await _userService.QueryUsers()
                .Where(u => u.Id == currentUserId)
                .FirstOrDefaultAsync();
                
            if (currentUser == null)
            {
                throw new AuthorizationException("Current user not found");
            }

            return currentUser;
        }

        [HttpGet]
        [Authorize]
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
                .Include(o => o.OrganisationUsers)
                    .ThenInclude(ou => ou.User)
                .ToListAsync();

            var formattedOrganisations = organisations
                .Select(org => new OrganisationResponseDTO
                {
                    OrgId = org.OrgId,
                    OrgName = org.OrgName,
                    CreatedAt = org.CreatedAt.ToString("dd/MM/yyyy"),
                    UsersCount = org.OrganisationUsers.Count(ou => !ou.IsDeleted && !ou.User.IsDeleted)
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
        [Authorize]
        public async Task<ActionResult<OrganisationResponseDTO>> GetOrganisationById(int id)
        {
            var organisation = await _organisationService.GetOrganisationByIdAsync(id);

            if (organisation == null)
            {
                throw new EntityNotFoundException($"Organisation with ID {id} not found.");
            }

            return Ok(organisation);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateOrganisation([FromBody] OrganisationCreateDTO orgDto)
        {
            // Get the current authenticated user for RBAC evaluation
            var actor = await GetCurrentUserAsync();

            // Console.WriteLine($"=== CREATE ORG RBAC DEBUG ===");
            // Console.WriteLine($"Actor role: '{actor.Role?.RoleName ?? "NULL"}'");
            // Console.WriteLine($"Organisation name: '{orgDto.OrgName}'");
            
            // Check RBAC permission for creating organisations
            var canCreate = _rbac.CanCreateOrganisation(actor);
            // Console.WriteLine($"RBAC CanCreateOrganisation result: {canCreate}");
            // Console.WriteLine($"============================");

            if (!canCreate)
                throw new AuthorizationException($"You are not allowed to create organisations. Your role is '{actor.Role?.RoleName ?? "NULL"}'.");

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

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateOrganisation(int id, [FromBody] OrganisationUpdateDTO orgDto)
        {
            // Get the current authenticated user for RBAC evaluation
            var actor = await GetCurrentUserAsync();

            // Get the target organisation to show in debug/error messages
            var targetOrg = await _organisationService.GetOrganisationByIdAsync(id);
            if (targetOrg == null)
            {
                throw new EntityNotFoundException($"Organisation with ID {id} not found.");
            }

            // Console.WriteLine($"=== UPDATE ORG RBAC DEBUG ===");
            // Console.WriteLine($"Actor role: '{actor.Role?.RoleName ?? "NULL"}'");
            // Console.WriteLine($"Target organisation: '{targetOrg.OrgName}' (ID: {targetOrg.OrgId})");
            
            // Check RBAC permission for updating organisations
            var canUpdate = _rbac.CanUpdateOrganisation(actor);
            // Console.WriteLine($"RBAC CanUpdateOrganisation result: {canUpdate}");
            // Console.WriteLine($"============================");

            if (!canUpdate)
                throw new AuthorizationException($"You are not allowed to update organisation '{targetOrg.OrgName}'. Your role is '{actor.Role?.RoleName ?? "NULL"}'.");

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

            var updatedOrg = await _organisationService.UpdateOrganisationAsync(id, orgDto);
            return Ok(updatedOrg);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteOrganisation(int id)
        {
            // Get the current authenticated user for RBAC evaluation
            var actor = await GetCurrentUserAsync();

            // Get the target organisation to show in debug/error messages
            var targetOrg = await _organisationService.GetOrganisationByIdAsync(id);
            if (targetOrg == null)
            {
                throw new EntityNotFoundException($"Organisation with ID {id} not found.");
            }

            // Console.WriteLine($"=== DELETE ORG RBAC DEBUG ===");
            // Console.WriteLine($"Actor role: '{actor.Role?.RoleName ?? "NULL"}'");
            // Console.WriteLine($"Target organisation: '{targetOrg.OrgName}' (ID: {targetOrg.OrgId})");
            
            // Check RBAC permission for deleting organisations
            var canDelete = _rbac.CanDeleteOrganisation(actor);
            // Console.WriteLine($"RBAC CanDeleteOrganisation result: {canDelete}");
            // Console.WriteLine($"============================");

            if (!canDelete)
                throw new AuthorizationException($"You are not allowed to delete organisation '{targetOrg.OrgName}'. Your role is '{actor.Role?.RoleName ?? "NULL"}'.");

            await _organisationService.DeleteOrganisationAsync(id);
            return NoContent();
        }
    }
}