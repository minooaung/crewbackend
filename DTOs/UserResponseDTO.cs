namespace crewbackend.DTOs
{
    public class UserResponseDTO
    {
        public int Id { get; set; }
        // public string Name { get; set;} = null!;
        // public string Email { get; set;} = null!;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public string? Created_at { get; set; }
        public string? Updated_at { get; set; }
        
        public string RoleName { get; set; } = string.Empty; 
    }
}