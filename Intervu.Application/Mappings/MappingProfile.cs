using AutoMapper;
using Intervu.Application.DTOs.Admin;
using Intervu.Application.DTOs.Availability;
using Intervu.Application.DTOs.Company;
using Intervu.Application.DTOs.Interviewee;
using Intervu.Application.DTOs.Interviewer;
using Intervu.Application.DTOs.Skill;
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

            CreateMap<User, DTOs.User.UserDto>();

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
            CreateMap<InterviewerProfile, InterviewerProfileDto>().ForMember(dest => dest.User, opt => opt.Ignore()).ReverseMap();
            CreateMap<InterviewerProfile, InterviewerViewDto>().ForMember(dest => dest.User, opt => opt.Ignore()).ReverseMap();
            CreateMap<InterviewerProfile, InterviewerCreateDto>().ReverseMap();
            CreateMap<InterviewerProfile, InterviewerUpdateDto>().ReverseMap();

            CreateMap<InterviewerProfileDto, InterviewerCreateDto>().ReverseMap();
            CreateMap<User, InterviewerCreateDto>().ReverseMap();

            CreateMap<Company, DTOs.Company.CompanyDto>().ReverseMap();
            CreateMap<Skill, SkillDto>().ReverseMap();

            // Interviewee mappings
            CreateMap<IntervieweeProfile, IntervieweeProfileDto>().ForMember(dest => dest.User, opt => opt.Ignore()).ReverseMap();
            CreateMap<IntervieweeProfile, IntervieweeViewDto>().ForMember(dest => dest.User, opt => opt.Ignore()).ReverseMap();
            CreateMap<IntervieweeCreateDto, IntervieweeProfile>()
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Skills, opt => opt.Ignore());
            CreateMap<IntervieweeProfile, IntervieweeCreateDto>();
            CreateMap<IntervieweeProfile, IntervieweeUpdateDto>().ReverseMap();

            // Admin mappings
            CreateMap<User, DTOs.Admin.UserDto>();
            CreateMap<Company, DTOs.Admin.CompanyDto>();
            //CreateMap<Payment, PaymentDto>();
            CreateMap<Feedback, FeedbackDto>();
            // InterviewerAdminDto is manually mapped in use case to include User data
            // Availability mappings
            CreateMap<InterviewerAvailabilityCreateDto, InterviewerAvailability>()
                .ForMember(dest => dest.IsBooked, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.Id, opt => opt.Ignore());

            CreateMap<InterviewerAvailabilityUpdateDto, InterviewerAvailability>()
                .ForMember(dest => dest.IsBooked, opt => opt.Ignore())
                .ForMember(dest => dest.InterviewerId, opt => opt.Ignore());
        }
    }
}
