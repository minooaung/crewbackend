using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CrewBackend.Data;
using CrewBackend.Models;

namespace CrewBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserRolesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserRolesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /api/userroles
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetAllRoles()
        {
            var roles = await _context.UserRoles
                .Select(r => new
                {
                    r.RoleId,
                    r.RoleName
                })
                .ToListAsync();

            return Ok(roles);
        }
    }
}
