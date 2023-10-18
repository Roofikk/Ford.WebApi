using AutoMapper;
using Ford.Models;
using Ford.WebApi.Dtos.Horse;
using System.Collections.ObjectModel;

namespace Ford.WebApi.Profiles
{
    public class HorseProfiles : Profile
    {
        public HorseProfiles()
        {
            CreateMap<Horse, HorseRetrievingDto>()
                .ForMember(
                    dest => dest.HorseId,
                    opt => opt.MapFrom(src => src.HorseId))
                .ForMember(
                    dest => dest.Name,
                    opt => opt.MapFrom(src => src.Name))
                .ForMember(
                    dest => dest.BirthDate,
                    opt => opt.MapFrom(src => src.BirthDate))
                .ForMember(
                    dest => dest.Sex,
                    opt => opt.MapFrom(src => src.Sex.ToString()))
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
                    dest => dest.CreationDate,
                    opt => opt.MapFrom(src => src.CreationDate))
                .ForMember(
                    dest => dest.Saves,
                    opt => opt.MapFrom(src => src.Saves))
                .ForMember(
                    dest => dest.Users,
                    opt => opt.MapFrom(src => src.Users));

            CreateMap<User, HorseUserDto>()
                .ForMember(
                    dest => dest.Id,
                    opt => opt.MapFrom(src => src.UserId))
                .ForMember(
                    dest => dest.Name,
                    opt => opt.MapFrom(src => src.Name))
                .ForMember(
                    dest => dest.LastName,
                    opt => opt.MapFrom(src => src.LastName))
                .ForMember(
                    dest => dest.Login,
                    opt => opt.MapFrom(src => src.Login));

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
                    opt => opt.MapFrom(src => new Collection<User>()))
                .ForMember(
                    dest => dest.CreationDate,
                    opt => opt.MapFrom(src => DateTime.Now));
        }
    }
}
