using System;

namespace crewbackend.Models
{
    public class OrganisationUser
    {    
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int OrgId { get; set; }
        public Organisation Organisation { get; set; } = null!;
    }
}