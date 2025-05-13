using crewbackend.DTOs;
using crewbackend.Models;
using crewbackend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using crewbackend.Helpers;

namespace crewbackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IUserService userService, ILogger<AuthController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        // [HttpPost("signup")]
        // public async Task<IActionResult> SignUp([FromBody] UserCreateDTO userDto)
        // {
        //     if (!ModelState.IsValid)
        //     {
        //         return BadRequest(ModelState);
        //     }

        //     var existingUser = await _userService.GetUserByEmailAsync(userDto.Email);
        //     if (existingUser != null)
        //     {
        //         return Conflict(new { message = "Email already in use" });
        //     }

        //     var createdUser = await _userService.CreateUserAsync(userDto);
        //     return CreatedAtAction(nameof(SignUp), new { id = createdUser.Id }, createdUser);
        // }

        [HttpPost("signup")]
        public async Task<IActionResult> SignUp([FromBody] UserCreateDTO userDto)
        {
            // if (!ModelState.IsValid)
            // {
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

            var existingUser = await _userService.GetUserByEmailAsync(userDto.Email);
            if (existingUser != null)
            {
                return Conflict(new { message = "Email already in use for another existing user" });
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

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDTO loginDto)
        {
            // if (!ModelState.IsValid)
            // {
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
                return Unauthorized(new { message = "Invalid email or password" });
            }

            // Create claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);            

            return Ok(new { user = user });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok(new { message = "Logout successful" });
        }
    }
}