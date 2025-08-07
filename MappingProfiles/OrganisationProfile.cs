using AutoMapper;
using crewbackend.DTOs;
using crewbackend.Models;

namespace crewbackend.MappingProfiles
{
    public class OrganisationProfile : Profile
    {
        public OrganisationProfile()
        {
            CreateMap<Organisation, OrganisationResponseDTO>()
                .ForMember(dest => dest.CreatedAt,
                    opt => opt.MapFrom(src => src.CreatedAt.ToString("dd/MM/yyyy")))
                .ForMember(dest => dest.UsersCount,
                    opt => opt.MapFrom(src => src.OrganisationUsers.Count));

            CreateMap<OrganisationCreateDTO, Organisation>();
            CreateMap<OrganisationUpdateDTO, Organisation>();
        }
    }
}