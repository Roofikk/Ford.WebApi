using AutoMapper;
using Ford.WebApi.Data.Entities;
using Ford.WebApi.Dtos.Horse;
using System.Collections.ObjectModel;

namespace Ford.WebApi.Properties;

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
                opt => opt.MapFrom(src => src.HorseOwners));

        CreateMap<HorseOwner, HorseUserDto>()
            .ForMember(
                dest => dest.Id,
                opt => opt.MapFrom(src => src.UserId))
            .ForMember(
                dest => dest.Email,
                opt => opt.MapFrom(src => src.User.UserName.ToString()))
            .ForMember(
                dest => dest.Name,
                opt => opt.MapFrom(src => src.User.FirstName))
            .ForMember(
                dest => dest.LastName,
                opt => opt.MapFrom(src => src.User.LastName));

        CreateMap<User, HorseUserDto>()
            .ForMember(
                dest => dest.Id,
                opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(
                dest => dest.Name,
                opt => opt.MapFrom(src => src.FirstName))
            .ForMember(
                dest => dest.LastName,
                opt => opt.MapFrom(src => src.LastName))
            .ForMember(
                dest => dest.Email,
                opt => opt.MapFrom(src => src.Email));

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
                dest => dest.HorseOwners,
                opt => opt.MapFrom(src => src.HorseOwners))
            .ForMember(
                dest => dest.CreationDate,
                opt => opt.MapFrom(src => DateTime.Now));

        CreateMap<HorseForUpdateDto, Horse>()
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
                opt => opt.MapFrom(src => string.IsNullOrEmpty(src.Sex) ? Sex.None : Enum.Parse<Sex>(src.Sex)))
            .ForMember(
                dest => dest.City,
                opt => opt.MapFrom(src => src.City))
            .ForMember(
                dest => dest.Region,
                opt => opt.MapFrom(src => src.Region))
            .ForMember(
                dest => dest.Country,
                opt => opt.MapFrom(src => src.Country));
    }
}
