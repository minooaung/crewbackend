using System;

namespace crewbackend.Models
{
    // [Index(nameof(Email), IsUnique = true)]
    public class User{
        public int Id { get; set; }
        // public string Name { get; set; } = null!;
        // public string Email { get; set; } = null!;
        // public string Password { get; set; } = null!;

        public required string Name { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Relationships
        public int RoleId { get; set; } // Foreign key for UserRole
        public UserRole Role { get; set; } = null!; // Navigation property for UserRole
        public ICollection<OrganisationUser> OrganisationUsers { get; set; } = new List<OrganisationUser>();
    }
}