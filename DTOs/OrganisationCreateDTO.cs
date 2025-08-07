using System.ComponentModel.DataAnnotations;

namespace crewbackend.DTOs
{
    public class OrganisationCreateDTO
    {
        [Required(ErrorMessage = "The organisation name is required.")]
        [StringLength(100, ErrorMessage = "The organisation name must not exceed 100 characters.")]
        public string OrgName { get; set; } = string.Empty;
    }
}