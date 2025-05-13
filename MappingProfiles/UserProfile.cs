using AutoMapper;
using crewbackend.DTOs;
using crewbackend.Models;

namespace crewbackend.MappingProfiles
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<User, UserResponseDTO>();
            CreateMap<UserCreateDTO, User>();
            CreateMap<UserUpdateDTO, User>()
            .ForMember(dest => dest.Password, opt => opt.Ignore()) // Prevent null overwrite
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore()); // Preserve original CreatedAt value
            //.ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
        }        
    }    
}