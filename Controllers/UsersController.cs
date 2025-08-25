using CrewBackend.DTOs;
using CrewBackend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using CrewBackend.Helpers;
using CrewBackend.Exceptions.Domain;
using CrewBackend.Exceptions.Auth;
using System.Security.Claims;

namespace CrewBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
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
        public async Task<ActionResult<UserResponseDTO>> GetUserById(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);

            if (user == null)
            {
                throw new EntityNotFoundException($"User with ID {id} not found.");
            }

            return Ok(user);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] UserCreateDTO userDto)
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

            var createdUser = await _userService.CreateUserAsync(userDto);
            return CreatedAtAction(nameof(GetUserById), new { id = createdUser.Id }, createdUser);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserUpdateDTO userDto)
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

            var updatedUser = await _userService.UpdateUserAsync(id, userDto);
            return Ok(updatedUser);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                throw new AuthorizationException("User ID not found in claims or invalid");
            }
            
            await _userService.DeleteUserAsync(id, userId);
            return NoContent();
        }

        [HttpGet("selected")]
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