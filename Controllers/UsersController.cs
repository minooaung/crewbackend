using CrewBackend.DTOs;
using CrewBackend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using CrewBackend.Helpers;
using CrewBackend.Exceptions.Domain;
using CrewBackend.Exceptions.Auth;
using System.Security.Claims;
using CrewBackend.Models;

namespace CrewBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IRbacPolicyEvaluator _rbac;

        public UsersController(IUserService userService, IRbacPolicyEvaluator rbac)
        {
            _userService = userService;
            _rbac = rbac;
        }

        /// <summary>
        /// Helper method to normalize role names to match UserRoleConstants
        /// </summary>
        private string NormalizeRoleName(string? roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
                return UserRoleConstants.Employee; // Default role
                
            var normalized = roleName.Trim();
            
            return normalized.ToUpperInvariant() switch
            {
                "EMPLOYEE" => UserRoleConstants.Employee,
                "ADMIN" => UserRoleConstants.Admin,
                "SUPERADMIN" => UserRoleConstants.SuperAdmin,
                _ => normalized // Return as-is if no match
            };
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

            // Additional debug - let's see if the role is loaded at this point
            // Console.WriteLine($"GetCurrentUserAsync - Role loaded: {currentUser.Role != null}");
            // if (currentUser.Role != null)
            // {
            //     Console.WriteLine($"GetCurrentUserAsync - Role Name: {currentUser.Role.RoleName}");
            // }

            return currentUser;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult> GetUsers(
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 10, 
            [FromQuery] string? search = null)
        {
            var query = _userService.QueryUsers();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u => u.Name.Contains(search) || u.Email.Contains(search));
            }

            var total = await query.CountAsync();
            var users = await query
                .OrderBy(u => u.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();            

            // Map users to DTOs with localized dates
            var formattedUsers = users
                                .Select(user => UserResponseMapper.MapToUserResponseDTO(user))
                                .ToList();

            // Pagination calculations
            var lastPage = (int)Math.Ceiling(total / (double)pageSize);
            var from = ((page - 1) * pageSize) + 1;
            var to = Math.Min(page * pageSize, total);

            // Prepare pagination links
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            
            var pageLinks = Enumerable.Range(1, lastPage)
                .Select(p => new PaginationLink{
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
                data = formattedUsers,
                links,
                meta
            });            
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<UserResponseDTO>> GetUserById(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);

            if (user == null)
            {
                throw new EntityNotFoundException($"User with ID {id} not found.");
            }

            return Ok(user);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateUser([FromBody] UserCreateDTO userDto)
        {
            // Get the current authenticated user for RBAC evaluation
            var actor = await GetCurrentUserAsync();

            // Normalize the target role to handle case mismatches
            var targetRole = NormalizeRoleName(userDto.Role);
            
            // Console.WriteLine($"=== RBAC DEBUG ===");
            // Console.WriteLine($"Raw userDto.Role: '{userDto.Role}'");
            // Console.WriteLine($"Normalized target role: '{targetRole}'");
            // Console.WriteLine($"Actor role: '{actor.Role?.RoleName ?? "NULL"}'");
            
            // Check RBAC permission for creating user with specified role
            var canCreate = _rbac.CanCreate(actor, targetRole);
            // Console.WriteLine($"RBAC CanCreate result: {canCreate}");
            // Console.WriteLine($"==================");

            if (!canCreate)
                throw new AuthorizationException($"You are not allowed to create users with role '{targetRole}'. Your role is '{actor.Role?.RoleName ?? "NULL"}'.");

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

            var createdUser = await _userService.CreateUserAsync(userDto);
            return CreatedAtAction(nameof(GetUserById), new { id = createdUser.Id }, createdUser);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserUpdateDTO userDto)
        {
            // Get the current authenticated user for RBAC evaluation
            var actor = await GetCurrentUserAsync();

            // Get the target user object for RBAC evaluation
            var target = await _userService.QueryUsers()
                .Where(u => u.Id == id)
                .FirstOrDefaultAsync();
            if (target == null)
            {
                throw new EntityNotFoundException($"User with ID {id} not found.");
            }

            // Console.WriteLine($"=== UPDATE RBAC DEBUG ===");
            // Console.WriteLine($"Actor role: '{actor.Role?.RoleName ?? "NULL"}'");
            // Console.WriteLine($"Target user: '{target.Name}' (ID: {target.Id})");
            // Console.WriteLine($"Target role: '{target.Role?.RoleName ?? "NULL"}'");
            // Console.WriteLine($"Is self-update: {actor.Id == target.Id}");

            // Check RBAC permission for updating this user
            var canUpdate = _rbac.CanUpdate(actor, target);
            // Console.WriteLine($"RBAC CanUpdate result: {canUpdate}");
            // Console.WriteLine($"========================");

            if (!canUpdate)
                throw new AuthorizationException($"You are not allowed to update user '{target.Name}' with role '{target.Role?.RoleName ?? "NULL"}'. Your role is '{actor.Role?.RoleName ?? "NULL"}'.");

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

            var updatedUser = await _userService.UpdateUserAsync(id, userDto);
            return Ok(updatedUser);
        }        

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteUser(int id)
        {
            // Get the current authenticated user for RBAC evaluation
            var actor = await GetCurrentUserAsync();

            // Get the target user object for RBAC evaluation
            var target = await _userService.QueryUsers()
                .Where(u => u.Id == id)
                .FirstOrDefaultAsync();
            if (target == null)
            {
                throw new EntityNotFoundException($"User with ID {id} not found.");
            }

            // Console.WriteLine($"=== DELETE RBAC DEBUG ===");
            // Console.WriteLine($"Actor role: '{actor.Role?.RoleName ?? "NULL"}'");
            // Console.WriteLine($"Target user: '{target.Name}' (ID: {target.Id})");
            // Console.WriteLine($"Target role: '{target.Role?.RoleName ?? "NULL"}'");
            // Console.WriteLine($"Is self-delete: {actor.Id == target.Id}");

            // Check RBAC permission for deleting this user
            var canDelete = _rbac.CanDelete(actor, target);
            // Console.WriteLine($"RBAC CanDelete result: {canDelete}");
            // Console.WriteLine($"========================");

            if (!canDelete)
                throw new AuthorizationException($"You are not allowed to delete user '{target.Name}' with role '{target.Role?.RoleName ?? "NULL"}'. Your role is '{actor.Role?.RoleName ?? "NULL"}'.");
            
            await _userService.DeleteUserAsync(id, actor.Id);
            return NoContent();
        }

        [HttpGet("selected")]
        [Authorize]
        public async Task<ActionResult> GetSelectedUsers([FromQuery] string ids)
        {
            if (string.IsNullOrEmpty(ids))
            {
                return Ok(new { data = new List<UserResponseDTO>() });
            }

            var userIds = ids.Split(',').Select(int.Parse).ToList();
            var query = _userService.QueryUsers().Where(u => userIds.Contains(u.Id));
            
            var users = await query.ToListAsync();
            var formattedUsers = users
                .Select(user => UserResponseMapper.MapToUserResponseDTO(user))
                .ToList();

            return Ok(new { data = formattedUsers });
        }
    }
}