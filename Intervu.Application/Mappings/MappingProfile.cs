using AutoMapper;
using Intervu.Application.DTOs.Admin;
using Intervu.Application.DTOs.Availability;
using Intervu.Application.DTOs.Candidate;
using Intervu.Application.DTOs.Coach;
using Intervu.Application.DTOs.Company;
using Intervu.Application.DTOs.InterviewType;
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

            // Coach mappings
            CreateMap<CoachProfile, CoachProfileDto>().ForMember(dest => dest.User, opt => opt.Ignore()).ReverseMap();
            CreateMap<CoachProfile, CoachViewDto>().ForMember(dest => dest.User, opt => opt.Ignore()).ReverseMap();
            CreateMap<CoachCreateDto, CoachProfile>().ReverseMap();
            CreateMap<CoachUpdateDto, CoachProfile>().ReverseMap();

            CreateMap<User, CoachCreateDto>().ReverseMap();

            CreateMap<Company, DTOs.Company.CompanyDto>().ReverseMap();
            CreateMap<Skill, SkillDto>().ReverseMap();

            // Candidate mappings
            CreateMap<CandidateProfile, CandidateProfileDto>().ForMember(dest => dest.User, opt => opt.Ignore()).ReverseMap();
            CreateMap<CandidateProfile, CandidateViewDto>().ForMember(dest => dest.User, opt => opt.Ignore()).ReverseMap();
            CreateMap<CandidateCreateDto, CandidateProfile>()
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Skills, opt => opt.Ignore());
            CreateMap<CandidateProfile, CandidateCreateDto>();
            CreateMap<CandidateProfile, CandidateUpdateDto>().ReverseMap();

            // Admin mappings
            CreateMap<User, DTOs.Admin.UserDto>();
            CreateMap<Company, DTOs.Admin.CompanyDto>();
            //CreateMap<Payment, PaymentDto>();
            CreateMap<Feedback, FeedbackDto>();
            // InterviewerAdminDto is manually mapped in use case to include User data
            // Availability mappings
            CreateMap<CoachAvailabilityCreateDto, CoachAvailability>()
                .ForMember(dest => dest.IsBooked, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.Id, opt => opt.Ignore());

            CreateMap<CoachAvailabilityUpdateDto, CoachAvailability>()
                .ForMember(dest => dest.IsBooked, opt => opt.Ignore())
                .ForMember(dest => dest.CoachId, opt => opt.Ignore());

            CreateMap<InterviewType, InterviewTypeDto>().ReverseMap();
        }
    }
}
