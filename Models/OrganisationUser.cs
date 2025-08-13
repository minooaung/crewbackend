using System;

namespace crewbackend.Models
{
    public class OrganisationUser
    {    
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int OrganisationId { get; set; }
        public Organisation Organisation { get; set; } = null!;

        public int? AssignedBy { get; set; }
        public User? AssignedByUser { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        // Soft delete properties
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public int? DeletedByUserId { get; set; }
        public User? DeletedByUser { get; set; }
    }
}