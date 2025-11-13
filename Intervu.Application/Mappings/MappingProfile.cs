using AutoMapper;
using Intervu.Application.DTOs.Interviewer;
using Intervu.Application.DTOs.User;
using Intervu.Domain.Entities;

namespace Intervu.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // User mappings
            CreateMap<User, LoginResponse>()
                .ForMember(dest => dest.Token, opt => opt.Ignore())
                .ForMember(dest => dest.ExpiresIn, opt => opt.Ignore());

            CreateMap<LoginRequest, User>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());

            // Register mappings
            CreateMap<RegisterRequest, User>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Password, opt => opt.Ignore()) // Will be hashed separately
                .ForMember(dest => dest.Role, opt => opt.Ignore()) // Will be parsed from string
                .ForMember(dest => dest.ProfilePicture, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore());

            // Interview mappings
            CreateMap<InterviewerProfile, InterviewerProfileDto>().ReverseMap();
            CreateMap<InterviewerProfile, InterviewerViewDto>().ReverseMap();
            CreateMap<InterviewerProfile, InterviewerCreateDto>().ReverseMap();
            CreateMap<InterviewerProfile, InterviewerUpdateDto>().ReverseMap();

            CreateMap<InterviewerCreateDto, InterviewerProfileDto>().ReverseMap();
            CreateMap<User, InterviewerCreateDto>().ReverseMap();
            CreateMap<User, InterviewerUpdateDto>().ReverseMap();
            CreateMap<User, InterviewerViewDto>().ReverseMap();
            CreateMap<User, InterviewerProfileDto>().ReverseMap();
            CreateMap<InterviewerUpdateDto, InterviewerProfileDto>().ReverseMap();

            CreateMap<InterviewerProfile, InterviewerUpdateDto>().ReverseMap()
                .ForMember(dest => dest.Status, opt => opt.Ignore()); // Prevent overwriting Status

            CreateMap<InterviewerUpdateDto, User>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.ProfilePicture, opt => opt.MapFrom(src => src.ProfilePicture)).ReverseMap();
        }
    }
}
