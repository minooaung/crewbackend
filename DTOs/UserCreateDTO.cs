using System.ComponentModel.DataAnnotations;
using CrewBackend.Validators;

namespace CrewBackend.DTOs
{
    public class UserCreateDTO
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StrongPassword]
        public string Password { get; set; } = string.Empty;


        [Required]
        // Don't use the following as custom validation check is done at UserService.cs
        //[Compare("Password", ErrorMessage = "Passwords do not match.")] 
        public string Password_Confirmation { get; set; } = string.Empty;
        
        public string? Role { get; set; }

    }
}