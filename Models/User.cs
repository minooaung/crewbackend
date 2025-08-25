using System;

namespace CrewBackend.Models
{
    // [Index(nameof(Email), IsUnique = true)]
    public class User{
        public int Id { get; set; }

        public required string Name { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Soft Delete Properties
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public int? DeletedByUserId { get; set; }
        public User? DeletedByUser { get; set; }

        // Relationships
        public int RoleId { get; set; } // Foreign key for UserRole
        public UserRole Role { get; set; } = null!; // Navigation property for UserRole
        public ICollection<OrganisationUser> OrganisationUsers { get; set; } = new List<OrganisationUser>();
    }
}