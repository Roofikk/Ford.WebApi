using AutoMapper;
using Ford.WebApi.Data.Entities;
using Ford.WebApi.Dtos.Save;

namespace Ford.WebApi.Profiles;

public class SaveProfile : Profile
{
    public SaveProfile()
    {
        //CreateMap<Save, ResponseSaveDto>()
        //    .ForMember(
        //        dest => dest.HorseId,
        //        opt => opt.MapFrom(src => src.HorseId))
        //    .ForMember(
        //        dest => dest.Header,
        //        opt => opt.MapFrom(src => src.Header))
        //    .ForMember(
        //        dest => dest.Date,
        //        opt => opt.MapFrom(src => src.Date))
        //    .ForMember(
        //        dest => dest.Bones,
        //        opt => opt.MapFrom(src => src.Bon))
    }
}
