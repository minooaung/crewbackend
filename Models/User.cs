using System;

namespace crewbackend.Models
{
    // [Index(nameof(Email), IsUnique = true)]
    public class User{
        public int Id { get; set;}
        public required string Name { get; set;}
        // public string Name { get; set; } = null!;
        public required string Email { get; set;}
        // public string Email { get; set; } = null!;
        public DateTime? EmailVerifiedAt { get; set; }
        public required string Password { get; set; }
        // public string Password { get; set; } = null!;
        public string? RememberToken { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    // public class User
    // {
    //     public int Id { get; set; }
    //     public string Name { get; set; }
    //     public string Email { get; set; }
    //     public DateTime? EmailVerifiedAt { get; set; }
    //     public string Password { get; set; }
    //     public string? RememberToken { get; set; }
    //     public DateTime CreatedAt { get; set; }
    //     public DateTime UpdatedAt { get; set; }
    // }
}