using System.Text.Json.Serialization;

namespace crewbackend.DTOs
{
    public class OrganisationResponseDTO
    {
        [JsonPropertyName("id")]
        public int OrgId { get; set; }

        [JsonPropertyName("name")]
        public string OrgName { get; set; } = string.Empty;

        [JsonPropertyName("created_at")]
        public string CreatedAt { get; set; } = string.Empty;

        [JsonPropertyName("users_count")]
        public int UsersCount { get; set; }
    }
}