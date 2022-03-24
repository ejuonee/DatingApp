using AutoMapper;
using DatingApp.DTO;
using DatingApp.Entities;
using DatingApp.Extensions;
using System;
using System.Linq;

namespace DatingApp.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<AppUser, MemberDto>().ForMember(dest => dest.PhotoUrl, opt => opt.MapFrom(src =>
                    src.Photos.FirstOrDefault(x => x.IsMain).Url))
                .ForMember(dest => dest.Age, opt => opt.MapFrom(src => src.DateOfBirth.CalculateAge())).ReverseMap();
            CreateMap<Photo, PhotoDto>();

            CreateMap<MemberUpdateDto,AppUser>().ReverseMap();
            CreateMap<RegisterDTO, AppUser>().ReverseMap();
            CreateMap<Message, MessageDto>().ForMember(dest=>dest.SenderPhotoUrl, opt=> opt.MapFrom(src=>src.Sender.Photos.FirstOrDefault(x=>x.IsMain).Url)).ForMember(dest=>dest.RecipientPhotoUrl, opt=> opt.MapFrom(src=>src.Recipient.Photos.FirstOrDefault(x=>x.IsMain).Url)).ReverseMap();
            CreateMap<DateTime, DateTime>().ConvertUsing(x=> DateTime.SpecifyKind(x,DateTimeKind.Utc));
        }
    }
}