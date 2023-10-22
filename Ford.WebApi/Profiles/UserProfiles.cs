using AutoMapper;
using Ford.WebApi.Data.Entities;
using Ford.WebApi.Dtos.User;

namespace Ford.WebApi.Profiles;

public class UserProfiles : Profile
{
    public UserProfiles()
    {
        CreateMap<UserCreationDto, User>()
            .ForMember(
                dest => dest.Id,
                opt => opt.MapFrom(src => Guid.NewGuid().ToString()))
            .ForMember(
                dest => dest.UserName,
                opt => opt.MapFrom(src => src.Login))
            .ForMember(
                dest => dest.Email,
                opt => opt.MapFrom(src => src.Email))
            .ForMember(
                dest => dest.FirstName,
                opt => opt.MapFrom(src => src.Name))
            .ForMember(
                dest => dest.LastName,
                opt => opt.MapFrom(src => src.LastName))
            .ForMember(
                dest => dest.Phone,
                opt => opt.MapFrom(src => src.Phone))
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
                dest => dest.BirthDate,
                opt => opt.MapFrom(src => src.BirthDate))
            .ForMember(
                dest => dest.CreationDate,
                opt => opt.MapFrom(src => DateTime.Now))
            .ForMember(
                dest => dest.LastUpdatedDate,
                opt => opt.MapFrom(src => DateTime.Now));

        CreateMap<UserForUpdateDto, User>()
            .ForMember(
                dest => dest.Id,
                opt => opt.MapFrom(src => Guid.Parse(src.Id)))
            .ForMember(
                dest => dest.Email,
                opt => opt.MapFrom(src => src.Email))
            .ForMember(
                dest => dest.FirstName,
                opt => opt.MapFrom(src => src.FirstName))
            .ForMember(
                dest => dest.LastName,
                opt => opt.MapFrom(src => src.LastName))
            .ForMember(
                dest => dest.Phone,
                opt => opt.MapFrom(src => src.Phone))
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
                dest => dest.BirthDate,
                opt => opt.MapFrom(src => src.BirthDate))
            .ForMember(
                dest => dest.LastUpdatedDate,
                opt => opt.MapFrom(src => DateTime.Now));

        CreateMap<User, UserGettingDto>()
            .ForMember(
                dest => dest.UserId,
                opt => opt.MapFrom(src => src.Id))
            .ForMember(
                dest => dest.Login,
                opt => opt.MapFrom(src => src.UserName))
            .ForMember(
                dest => dest.Email,
                opt => opt.MapFrom(src => src.Email))
            .ForMember(
                dest => dest.FirstName,
                opt => opt.MapFrom(src => src.FirstName))
            .ForMember(
                dest => dest.LastName,
                opt => opt.MapFrom(src => src.LastName))
            .ForMember(
                dest => dest.Phone,
                opt => opt.MapFrom(src => src.Phone))
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
                dest => dest.BirthDate,
                opt => opt.MapFrom(src => src.BirthDate))
            .ForMember(
                dest => dest.CreationDate,
                opt => opt.MapFrom(src => src.CreationDate))
            .ForMember(
                dest => dest.LastUpdatedDate,
                opt => opt.MapFrom(src => src.LastUpdatedDate));
    }
}
