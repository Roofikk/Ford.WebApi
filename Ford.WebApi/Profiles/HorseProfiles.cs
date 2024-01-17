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
                dest => dest.Description,
                opt => opt.MapFrom(src => src.Description))
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
                dest => dest.Users,
                opt => opt.MapFrom(src => src.HorseOwners));

        CreateMap<HorseOwner, OwnerDto>()
            .ForMember(
                dest => dest.Id,
                opt => opt.MapFrom(src => src.UserId))
            .ForMember(
                dest => dest.RuleAccess,
                opt => opt.MapFrom(src => src.RuleAccess))
            .ForMember(
                dest => dest.FirstName,
                opt => opt.MapFrom(src => src.User.FirstName))
            .ForMember(
                dest => dest.LastName,
                opt => opt.MapFrom(src => src.User.LastName));

        CreateMap<User, OwnerDto>()
            .ForMember(
                dest => dest.Id,
                opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(
                dest => dest.FirstName,
                opt => opt.MapFrom(src => src.FirstName))
            .ForMember(
                dest => dest.LastName,
                opt => opt.MapFrom(src => src.LastName));

        CreateMap<HorseForCreationDto, Horse>()
            .ForMember(
                dest => dest.Name,
                opt => opt.MapFrom(src => src.Name))
            .ForMember(
                dest => dest.Description,
                opt => opt.MapFrom(src => src.Description))
            .ForMember(
                dest => dest.BirthDate,
                opt => opt.MapFrom(src => src.BirthDate))
            .ForMember(
                dest => dest.Sex,
                opt => opt.MapFrom(src => src.Sex))
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
                opt => opt.MapFrom(src => DateTime.Now))
            .ForMember(
                dest => dest.Saves,
                opt => opt.MapFrom(src => new Collection<Save>()))
            .ForMember(
                dest => dest.HorseOwners,
                opt => opt.MapFrom(src => new Collection<HorseOwner>()));

        CreateMap<HorseForUpdateDto, Horse>()
            .ForMember(
                dest => dest.HorseId,
                opt => opt.MapFrom(src => src.HorseId))
            .ForMember(
                dest => dest.Name,
                opt => opt.MapFrom(src => src.Name))
            .ForMember(
                dest => dest.Description,
                opt => opt.MapFrom(src => src.Description))
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
