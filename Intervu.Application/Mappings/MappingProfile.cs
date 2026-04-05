using AutoMapper;
using Intervu.Application.DTOs.Admin;
using Intervu.Application.DTOs.Availability;
using Intervu.Application.DTOs.BookingRequest;
using Intervu.Application.DTOs.Candidate;
using Intervu.Application.DTOs.Coach;
using Intervu.Application.DTOs.CoachInterviewService;
using Intervu.Application.DTOs.Comment;
using Intervu.Application.DTOs.Company;
using Intervu.Application.DTOs.Feedback;
using Intervu.Application.DTOs.Industry;
using Intervu.Application.DTOs.InterviewExperience;
using Intervu.Application.DTOs.InterviewType;
using Intervu.Application.DTOs.Question;
using Intervu.Application.DTOs.Skill;
using Intervu.Application.DTOs.User;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Entities.Constants.QuestionConstants;
using System.Collections.Generic;
using System.Linq;

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
                .ForMember(dest => dest.Password, opt => opt.Ignore())
                .ForMember(dest => dest.Role, opt => opt.Ignore())
                .ForMember(dest => dest.ProfilePicture, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore());

            // Coach mappings
            CreateMap<CoachProfile, CoachProfileDto>().ForMember(dest => dest.User, opt => opt.Ignore()).ReverseMap();
            CreateMap<CoachProfile, CoachViewDto>().ForMember(dest => dest.User, opt => opt.Ignore()).ReverseMap();
            CreateMap<CoachCreateDto, CoachProfile>().ReverseMap();
            CreateMap<CoachUpdateDto, CoachProfile>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Skills, opt => opt.Ignore())
            .ForMember(dest => dest.Companies, opt => opt.Ignore())
            .ForMember(dest => dest.Industries, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore())
    .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<CoachUpdateDto, CoachProfile>().ForMember(dest => dest.Id, opt => opt.Ignore());

            CreateMap<User, CoachCreateDto>().ReverseMap();

            CreateMap<Company, DTOs.Company.CompanyDto>().ReverseMap();
            CreateMap<Skill, SkillDto>().ReverseMap();
            CreateMap<Industry, IndustryDto>().ReverseMap();

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
            CreateMap<User, DTOs.Admin.AdminUserResponseDto>();
            CreateMap<Company, DTOs.Admin.CompanyDto>();
            CreateMap<Feedback, FeedbackDto>();
            CreateMap<Feedback, GetFeedbackResponse>()
                .ForMember(dest => dest.FeedbackId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.CoachName, opt => opt.MapFrom(src =>
                    src.CoachProfile != null && src.CoachProfile.User != null ? src.CoachProfile.User.FullName : string.Empty))
                .ForMember(dest => dest.ScheduledTime, opt => opt.MapFrom(src =>
                    src.InterviewRoom != null ? src.InterviewRoom.ScheduledTime : (DateTime?)null))
                .ForMember(dest => dest.DurationMinutes, opt => opt.MapFrom(src =>
                    src.InterviewRoom != null ? src.InterviewRoom.DurationMinutes : (int?)null));

            // Availability mappings
            CreateMap<CoachAvailabilityCreateDto, CoachAvailability>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());

            CreateMap<CoachAvailabilityUpdateDto, CoachAvailability>()
                .ForMember(dest => dest.CoachId, opt => opt.Ignore());

            CreateMap<InterviewType, InterviewTypeDto>().ReverseMap();

            //InterviewExperience mappings 

            CreateMap<Comment, CommentDto>()
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreateBy));

            CreateMap<Question, QuestionDto>()
                .ForMember(dest => dest.Comments,
                    opt => opt.MapFrom(src => src.Comments != null
                        ? src.Comments.OrderByDescending(c => c.IsAnswer).ThenByDescending(c => c.Vote).ThenBy(c => c.CreatedAt).ToList()
                        : new List<Comment>()));

            CreateMap<Tag, TagDto>();

            CreateMap<Domain.Entities.InterviewExperience, InterviewExperienceSummaryDto>()
                .ForMember(dest => dest.CompanyName,
                    opt => opt.MapFrom(src => src.Company != null ? src.Company.Name : string.Empty))
                .ForMember(dest => dest.ContributorId,
                    opt => opt.MapFrom(src => src.CreatedBy))
                .ForMember(dest => dest.QuestionCount,
                    opt => opt.MapFrom(src => src.Questions != null ? src.Questions.Count : 0));

            CreateMap<Domain.Entities.InterviewExperience, InterviewExperienceDetailDto>()
                .IncludeBase<Domain.Entities.InterviewExperience, InterviewExperienceSummaryDto>()
                .ForMember(dest => dest.Questions,
                    opt => opt.MapFrom(src => src.Questions));

            CreateMap<CreateInterviewExperienceRequest, Domain.Entities.InterviewExperience>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CompanyId, opt => opt.MapFrom(src => src.CompanyId))
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Company, opt => opt.Ignore())
                .ForMember(dest => dest.Questions, opt => opt.Ignore());
            // CoachInterviewService mappings
            CreateMap<CoachInterviewService, CoachInterviewServiceDto>()
                .ForMember(dest => dest.InterviewTypeName, opt => opt.MapFrom(src => src.InterviewType.Name))
                .ForMember(dest => dest.IsCoding, opt => opt.MapFrom(src => src.InterviewType.IsCoding));
            CreateMap<CreateCoachInterviewServiceDto, CoachInterviewService>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CoachId, opt => opt.Ignore());

            // BookingRequest mappings
            CreateMap<Domain.Entities.BookingRequest, BookingRequestDto>()
                .ForMember(dest => dest.CandidateName, opt => opt.Ignore())
                .ForMember(dest => dest.CoachName, opt => opt.Ignore())
                .ForMember(dest => dest.InterviewTypeName, opt => opt.MapFrom(src =>
                    src.CoachInterviewService != null ? src.CoachInterviewService.InterviewType.Name : null))
                .ForMember(dest => dest.ServicePrice, opt => opt.MapFrom(src =>
                    src.CoachInterviewService != null ? src.CoachInterviewService.Price : (int?)null))
                .ForMember(dest => dest.ServiceDurationMinutes, opt => opt.MapFrom(src =>
                    src.CoachInterviewService != null ? src.CoachInterviewService.DurationMinutes : (int?)null));

            // InterviewRound mappings
            CreateMap<Domain.Entities.InterviewRound, InterviewRoundDto>()
                .ForMember(dest => dest.InterviewRoomId, opt => opt.MapFrom(src => src.InterviewRoomId))
                .ForMember(dest => dest.InterviewRoomStatus, opt => opt.MapFrom(src => 
                    src.InterviewRoom != null ? src.InterviewRoom.Status.ToString() : null))
                .ForMember(dest => dest.InterviewTypeName, opt => opt.MapFrom(src =>
                    src.CoachInterviewService.InterviewType.Name))
                .ForMember(dest => dest.IsCoding, opt => opt.MapFrom(src =>
                    src.CoachInterviewService.InterviewType.IsCoding));
        }
    }
}
