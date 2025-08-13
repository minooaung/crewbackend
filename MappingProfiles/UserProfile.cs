using AutoMapper;
using crewbackend.DTOs;
using crewbackend.Models;

namespace crewbackend.MappingProfiles
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<User, UserResponseDTO>()
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.RoleName.ToUpper()))
            .ForMember(dest => dest.Created_at, opt => opt.MapFrom(src => src.CreatedAt.HasValue ? src.CreatedAt.Value.ToString("dd/MM/yyyy") : string.Empty));

            CreateMap<UserCreateDTO, User>()
                .ForMember(dest => dest.Role, opt => opt.Ignore())
                .ForMember(dest => dest.RoleId, opt => opt.Ignore());

            // Shows clearly what properties are ignored or specially mapped
            CreateMap<UserUpdateDTO, User>()
            .ForMember(dest => dest.Password, opt => opt.Ignore()) // Prevent null overwrite
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore()); // Preserve original CreatedAt value
            //.ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
        }        
    }    
}