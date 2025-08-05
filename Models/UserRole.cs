using System;

namespace crewbackend.Models
{
    public class UserRole
    {
        public int RoleId { get; set; }
        // public string RoleName { get; set; } = null!;
        public required string RoleName { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public ICollection<User> Users { get; set; } = new List<User>();
    }

}