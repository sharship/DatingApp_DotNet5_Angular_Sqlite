using System;
using System.Linq;
using API.DTOs;
using API.Entities;
using AutoMapper;
using Extensions;

namespace API.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            // Member
            CreateMap<MemberUpdateDto, AppUser>();
            CreateMap<AppUser, MemberDto>()
                .ForMember(
                    dest => dest.PhotoUrl, 
                    opt => opt.MapFrom(
                        src => src.Photos.FirstOrDefault(p => p.IsMain).Url
                    )
                )
                .ForMember(
                    dest => dest.Age,
                    opt => opt.MapFrom(
                        src => src.DateOfBirth.CalculateAge()
                    )
                );

            CreateMap<Photo, PhotoDto>();
            CreateMap<RegisterDto, AppUser>();

            // Message
            CreateMap<Message, MessageDto>()
                .ForMember(
                    dest => dest.SenderPhotoUrl,
                    opt => opt.MapFrom(
                        src => src.Sender.Photos.FirstOrDefault(p => p.IsMain).Url
                    )
                )
                .ForMember(
                    dest => dest.RecipientPhotoUrl,
                    opt => opt.MapFrom(
                        src => src.Recipient.Photos.FirstOrDefault(p => p.IsMain).Url
                    )
                );
            
            // CreateMap<DateTime, DateTime>().ConvertUsing(d => DateTime.SpecifyKind(d, DateTimeKind.Utc));
        }
    }
}