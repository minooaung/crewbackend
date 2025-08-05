using System;

namespace crewbackend.Models
{
    public class Organisation
    {
        public int OrgId { get; set; }
        
        // public string OrgName { get; set; } = null!;
        public required string OrgName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public ICollection<OrganisationUser> OrganisationUsers { get; set; } = new List<OrganisationUser>();
    }
}