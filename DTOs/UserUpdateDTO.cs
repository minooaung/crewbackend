using System.ComponentModel.DataAnnotations;
using CrewBackend.Validators;

namespace CrewBackend.DTOs
{
    public class UserUpdateDTO
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        // Password is optional for update, but can add validation if needed
        [StrongPassword]
        public string? Password { get; set; } // Optional

        // Don't use the following as custom validation check is done at UserService.cs
        //[Compare("Password")]
        public string? Password_Confirmation { get; set; }  // Optional
    }
}