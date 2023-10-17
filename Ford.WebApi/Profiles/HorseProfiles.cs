using AutoMapper;
using Ford.Models;
using Ford.WebApi.Dtos.Horse;

namespace Ford.WebApi.Profiles
{
    public class HorseProfiles : Profile
    {
        public HorseProfiles()
        {
            CreateMap<HorseForCreationDto, Horse>()
                .ForMember(
                    dest => dest.Name,
                    opt => opt.MapFrom(src => src.Name))
                .ForMember(
                    dest => dest.BirthDate,
                    opt => opt.MapFrom(src => src.BirthDate))
                .ForMember(
                    dest => dest.Sex,
                    opt => opt.MapFrom(src => string.IsNullOrEmpty(src.Sex) ? Sex.None : Enum.Parse<Sex>(src.Sex)))
                .ForMember(
                    dest => dest.City,
                    opt => opt.MapFrom(src => src.City))
                .ForMember(
                    dest => dest.Region,
                    opt => opt.MapFrom(src => src.Region))
                .ForMember(
                    dest => dest.Country,
                    opt => opt.MapFrom(src => src.Country))
                .ForMember(
                    dest => dest.Users,
                    opt => opt.MapFrom(src => src.Users));
        }
    }
}
