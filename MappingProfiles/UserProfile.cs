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
            .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role.RoleName))
            .ForMember(dest => dest.Created_at, opt => opt.MapFrom(src => src.CreatedAt.HasValue ? src.CreatedAt.Value.ToString("yyyy-MM-dd HH:mm:ss") : null))
            .ForMember(dest => dest.Updated_at, opt => opt.MapFrom(src => src.UpdatedAt.HasValue ? src.UpdatedAt.Value.ToString("yyyy-MM-dd HH:mm:ss") : null));

            CreateMap<UserCreateDTO, User>();

            CreateMap<UserUpdateDTO, User>()
            .ForMember(dest => dest.Password, opt => opt.Ignore()) // Prevent null overwrite
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore()); // Preserve original CreatedAt value
            //.ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
        }        
    }    
}