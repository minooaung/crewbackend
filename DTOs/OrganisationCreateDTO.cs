using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace crewbackend.DTOs
{
    public class OrganisationCreateDTO
    {
        [Required(ErrorMessage = "The organisation name is required.")]
        [StringLength(100, ErrorMessage = "The organisation name must not exceed 100 characters.")]
        [JsonPropertyName("name")]
        public string OrgName { get; set; } = string.Empty;

        [JsonPropertyName("user_ids")]
        public List<int> UserIds { get; set; } = new List<int>();
    }
}