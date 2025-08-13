using crewbackend.DTOs;
using crewbackend.Models;
using crewbackend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using AutoMapper;
using crewbackend.Helpers;
using CrewBackend.Exceptions.Domain;
using CrewBackend.Exceptions.Auth;

namespace crewbackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<AuthController> _logger;
        private readonly IMapper _mapper;

        public AuthController(IUserService userService, ILogger<AuthController> logger, IMapper mapper)
        {
            _userService = userService;
            _logger = logger;
            _mapper = mapper;
        }

        [HttpPost("signup")]
        public async Task<IActionResult> SignUp([FromBody] UserCreateDTO userDto)
        {
            ControllerHelpers.HandleModelStateErrors(ModelState);

            var existingUser = await _userService.GetUserByEmailAsync(userDto.Email);
            if (existingUser != null)
            {
                throw new ValidationException("email", "The email has already been taken.");
            }

            var createdUser = await _userService.CreateUserAsync(userDto);

            // Sign in the newly created user
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, createdUser.Id.ToString()),
                new Claim(ClaimTypes.Name, createdUser.Name),
                new Claim(ClaimTypes.Email, createdUser.Email)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);            

            return Ok(new { user = createdUser });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDTO loginDto)
        {
            ControllerHelpers.HandleModelStateErrors(ModelState);

            // Log incoming headers and cookies (for debugging)
            foreach (var header in Request.Headers)
            {
                _logger.LogInformation("Header: {Key} = {Value}", header.Key, header.Value);
            }

            foreach (var cookie in Request.Cookies)
            {
                _logger.LogInformation("Cookie: {Key} = {Value}", cookie.Key, cookie.Value);
            }

            var user = await _userService.AuthenticateAsync(loginDto.Email, loginDto.Password);

            if (user == null)
            {
                throw new AuthenticationException("These credentials do not match our records.");
            }

            // Create claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.RoleName)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

            var userDto = UserResponseMapper.MapToUserResponseDTO(user);
            return Ok(new { user = userDto });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok(new { message = "Logout successful" });
        }
    }
}