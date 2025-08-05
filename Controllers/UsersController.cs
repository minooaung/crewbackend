using crewbackend.DTOs;
using crewbackend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using crewbackend.Helpers;

// using Microsoft.Identity.Client;

namespace crewbackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        // Constructor injection happens here
        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        // GET: api/users
        // [HttpGet]
        // public async Task<ActionResult<IEnumerable<UserResponseDTO>>> GetUsers()
        // {
        //     var users = await _userService.GetAllUsersAsync();        
        //     return Ok(new { users = users });
        // }

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

            //return Ok(formattedUsers);

            // Pagination calculations
            var lastPage = (int)Math.Ceiling(total / (double)pageSize);
            var from = ((page - 1) * pageSize) + 1;
            var to = Math.Min(page * pageSize, total);

            // Prepare pagination links
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";

            var pageLinks = Enumerable.Range(1, lastPage)
                .Select(p => new PaginationLink{
                    //Url = p == page ? null : $"{baseUrl}?page={p}",
                    Url = $"{baseUrl}?page={p}",
                    Label = p.ToString(),
                    Active = p == page
                }).ToList();

            pageLinks.Insert(0, new PaginationLink
            {
                Url = page > 1 ? $"{baseUrl}?page={page - 1}" : null,
                Label = "<< Previous",
                Active = false
            });

            pageLinks.Add(new PaginationLink
            {
                Url = page < lastPage ? $"{baseUrl}?page={page + 1}" : null,
                Label = "Next >>",
                Active = false
            });

            // setPaginationLinks(response.data.meta.links);
            // setCurrentPage(response.data.meta.current_page);
            // setTotalUsers(response.data.meta.total);
            // setFromUser(response.data.meta.from);
            // setToUser(response.data.meta.to);

            var meta = new
            {
                links = pageLinks,
                current_page = page,
                total,
                from,
                to,                
            };

            // var meta = new
            // {
            //     current_page = page,
            //     last_page = lastPage,
            //     per_page = pageSize,
            //     from,  
            //     to,              
            //     path = baseUrl,
            //     total,
            //     links = pageLinks
            // };

            // var links = new
            // {
            //     first = $"{baseUrl}?page=1",
            //     last = $"{baseUrl}?page={lastPage}",
            //     prev = page > 1 ? $"{baseUrl}?page={page - 1}" : null,
            //     next = page < lastPage ? $"{baseUrl}?page={page + 1}" : null
            // };

            return Ok(new
            {
                users = formattedUsers,
                meta,
                // links
            });            
        }

        // GET: api/users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserResponseDTO>> GetUserById(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);

            if (user == null) return NotFound();

            return Ok(user);
        }

        // POST: api/users
        [Authorize(Roles = "Admin")]
        [HttpPost]
        //public async Task<ActionResult<UserResponseDTO>> CreateUser([FromBody] UserCreateDTO userDto)
        public async Task<IActionResult> CreateUser([FromBody] UserCreateDTO userDto)
        {
            // var errorResult = ControllerHelpers.HandleModelStateErrors(ModelState);
            // if (errorResult != null)
            // {
            //     return errorResult;
            // }

            // //var createdUser = await _userService.CreateUserAsync(userDto);
            // //return CreatedAtAction(nameof(GetUser), new { id = createdUser.Id }, createdUser);

            // var createdUser = await _userService.CreateUserAsync(userDto);
            // return Ok(new { user = createdUser });
            
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var createdUser = await _userService.CreateUserAsync(userDto);
            return CreatedAtAction(nameof(GetUserById), new { id = createdUser.Id }, createdUser);
        }

        // PUT: api/users/5
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserUpdateDTO userDto)
        {
            // if (!ModelState.IsValid)
            // {
            //     //return BadRequest(ModelState);
            //     var errors = ModelState
            //         .Where(x => x.Value != null && x.Value.Errors.Count > 0)
            //         .ToDictionary(
            //             kvp => kvp.Key,
            //             kvp => kvp.Value?.Errors?.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
            //         );

            //     return BadRequest(new { errors });
            // }

            var errorResult = ControllerHelpers.HandleModelStateErrors(ModelState);
            if (errorResult != null)
            {
                return errorResult;
            }

            var success = await _userService.UpdateUserAsync(id, userDto);
            if (!success) return NotFound();
            return NoContent();
        }

        // DELETE: api/users/5
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var success = await _userService.DeleteUserAsync(id);
            if (!success) return NotFound(); 
            return NoContent();
        }
    }
}