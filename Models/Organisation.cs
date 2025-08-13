using System;

namespace crewbackend.Models
{
    public class Organisation
    {
        public int OrgId { get; set; }        
        
        public required string OrgName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Soft Delete Properties
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public int? DeletedByUserId { get; set; }
        public User? DeletedByUser { get; set; }

        public ICollection<OrganisationUser> OrganisationUsers { get; set; } = new List<OrganisationUser>();
    }
}