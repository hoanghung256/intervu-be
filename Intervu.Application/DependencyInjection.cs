using Intervu.Application.Mappings;
using Intervu.Application.Services;
using Intervu.Application.UseCases.Authentication;
using Intervu.Application.Interfaces.UseCases.Authentication;
using Intervu.Application.Interfaces.UseCases.InterviewRoom;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Intervu.Application.UseCases.CoachProfile;
using Intervu.Application.Interfaces.UseCases.CoachProfile;
using Intervu.Application.UseCases.Coach;
using Intervu.Application.Interfaces.UseCases.Coach;
using CompanyInterfaces = Intervu.Application.Interfaces.UseCases.Company;
using CompanyUseCases = Intervu.Application.UseCases.Company;
using Intervu.Application.Interfaces.UseCases.Skill;
using Intervu.Application.UseCases.Skill;
using Intervu.Application.Interfaces.UseCases.Admin;
using AdminUseCases = Intervu.Application.UseCases.Admin;
using Intervu.Application.Interfaces.UseCases.Availability;
using Intervu.Application.UseCases.Availability;
using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.Interfaces.UseCases.Candidate;
using Intervu.Application.Interfaces.UseCases.CandidateProfile;
using Intervu.Application.Services.CodeGeneration;
// removed duplicate using Intervu.Application.Interfaces.UseCases.InterviewRoom
using Intervu.Application.Interfaces.UseCases.Email;
using Intervu.Application.UseCases.Email;
using Intervu.Application.Interfaces.UseCases.Feedbacks;
using Intervu.Application.UseCases.Feedbacks;
using Intervu.Application.Interfaces.UseCases.UserProfile;
using Intervu.Application.UseCases.UserProfile;
using Intervu.Application.Interfaces.UseCases.InterviewBooking;
using Intervu.Application.Interfaces.UseCases.InterviewType;
using Intervu.Application.UseCases.InterviewBooking;
using Intervu.Application.UseCases.InterviewRoom;
using Intervu.Application.Utils;
using Intervu.Application.UseCases.Admin;
using Intervu.Application.Interfaces.UseCases.PasswordReset;
using Intervu.Application.UseCases.PasswordReset;
using Intervu.Application.UseCases.Candidate;
using Intervu.Application.UseCases.CandidateProfile;
using Intervu.Application.Interfaces.UseCases.RescheduleRequest;
using Intervu.Application.UseCases.RescheduleRequest;
using Intervu.Application.UseCases.InterviewType;
using Intervu.Application.Interfaces.UseCases.InterviewExperience;
using Intervu.Application.UseCases.InterviewExperience;
using Intervu.Application.Interfaces.UseCases.Question;
using Intervu.Application.UseCases.Question;
using Intervu.Application.Interfaces.UseCases.Comment;
using Intervu.Application.UseCases.Comment;
using Intervu.Domain.Abstractions.Policies.Interfaces;
using Intervu.Domain.Abstractions.Policies;
using BookingRequestInterfaces = Intervu.Application.Interfaces.UseCases.BookingRequest;
using BookingRequestUseCases = Intervu.Application.UseCases.BookingRequest;
using CoachServiceInterfaces = Intervu.Application.Interfaces.UseCases.CoachInterviewService;
using CoachServiceUseCases = Intervu.Application.UseCases.CoachInterviewService;
using Intervu.Application.Interfaces.UseCases.Industry;
using Intervu.Application.UseCases.Industry;

namespace Intervu.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddUseCases(this IServiceCollection services, IConfiguration configuration)
        {
            // AutoMapper configuration
            services.AddAutoMapper(typeof(DependencyInjection));

            // Register Services
            services.AddScoped<JwtService>();
            services.AddSingleton<RoomManagerService>();
            services.AddSingleton<InterviewRoomCache>();
            services.AddSingleton<ICodeGenerationService, CSharpCodeGenerationService>();
            services.AddSingleton<ICodeGenerationService, JavaScriptCodeGenerationService>();
            services.AddSingleton<ICodeGenerationService, JavaCodeGenerationService>();

            // Auth UseCases
            services.AddTransient<ILoginUseCase, LoginUseCase>();
            services.AddTransient<IGoogleLoginUseCase, GoogleLoginUseCase>();
            services.AddTransient<IRegisterUseCase, RegisterUseCase>();
            services.AddTransient<IRefreshTokenUseCase, RefreshTokenUseCase>();

            // Password Reset UseCases
            services.AddTransient<IForgotPasswordUseCase, ForgotPasswordUseCase>();
            services.AddTransient<IValidateResetTokenUseCase, ValidateResetTokenUseCase>();
            services.AddTransient<IResetPasswordUseCase, ResetPasswordUseCase>();

            // ----- InterviewRoom ----
            services.AddScoped<ICreateInterviewRoom, CreateInterviewRoom>();
            services.AddScoped<IGetRoomHistory, GetRoomHistory>();
            services.AddScoped<IUpdateRoom, UpdateRoom>();
            services.AddScoped<IGetCurrentRoom, GetCurrentRoom>();
            services.AddScoped<IGetCoachEvaluation, GetCoachEvaluation>();
            services.AddScoped<ISubmitCoachEvaluation, SubmitCoachEvaluation>();
            // ----- CoachProfile ----
            services.AddScoped<ICreateCoachProfile, CreateCoachProfile>();
            services.AddScoped<IUpdateCoachProfile, UpdateCoachProfile>();
            services.AddScoped<IViewCoachProfile, ViewCoachProfile>();
            services.AddScoped<IDeleteCoachProfile, DeleteCoachProfile>();
            services.AddScoped<IGetAllCoach, GetAllCoach>();
            services.AddScoped<CompanyInterfaces.IGetAllCompanies, CompanyUseCases.GetAllCompanies>();
            services.AddScoped<IGetAllSkills, GetAllSkills>();
            services.AddScoped<IGetAllIndustries, GetAllIndustries>();
            // ----- Admin ----
            services.AddScoped<IGetDashboardStats, AdminUseCases.GetDashboardStats>();
            services.AddScoped<IGetAllUsersForAdmin, AdminUseCases.GetAllUsers>();
            services.AddScoped<IFilterUsersForAdmin, AdminUseCases.FilterUsersForAdmin>();
            services.AddScoped<IGetAllCompaniesForAdmin, AdminUseCases.GetAllCompanies>();
            services.AddScoped<IGetAllPayments, AdminUseCases.GetAllPayments>();
            services.AddScoped<IGetAllFeedbacks, AdminUseCases.GetAllFeedbacks>();
            services.AddScoped<IGetAllCoachForAdmin, AdminUseCases.GetAllCoachForAdmin>();
            services.AddScoped<ICreateUserForAdmin, AdminUseCases.CreateUserForAdmin>();
            services.AddScoped<IGetUserByIdForAdmin, AdminUseCases.GetUserByIdForAdmin>();
            services.AddScoped<IUpdateUserForAdmin, AdminUseCases.UpdateUserForAdmin>();
            services.AddScoped<IDeleteUserForAdmin, AdminUseCases.DeleteUserForAdmin>();
            services.AddScoped<IActivateUserForAdmin, AdminUseCases.ActivateUserForAdmin>();
            // ----- Feedback ----
            services.AddScoped<IGetFeedbacks, GetFeedbacks>();
            services.AddScoped<ICreateFeedback, CreateFeedback>();
            services.AddScoped<IUpdateFeedback, UpdateFeedback>();

            // ----- Coach Availability ----
            services.AddScoped<IGetCoachAvailabilities, GetCoachAvailabilities>();
            services.AddScoped<IGetCoachFreeSlots, GetCoachFreeSlots>();
            services.AddScoped<ICreateCoachAvailability, CreateCoachAvailability>();
            services.AddScoped<IDeleteCoachAvailability, DeleteCoachAvailability>();
            services.AddScoped<IUpdateAvailabilityStatus, UpdateAvailabilityStatus>();
            services.AddScoped<IUpdateCoachAvailability, UpdateCoachAvailability>();
            // ----- Email ----
            services.AddScoped<ISendBookingConfirmationEmail, SendBookingConfirmationEmail>();

            // ----- UserProfile ----
            services.AddScoped<IGetUserProfile, GetUserProfile>();
            services.AddScoped<IUpdateUserProfile, UpdateUserProfile>();
            services.AddScoped<IChangePassword, ChangePassword>();
            services.AddScoped<IUploadAvatar, UploadAvatar>();
            services.AddScoped<IClearProfilePicture, ClearProfilePicture>();
            services.AddScoped<IUpdateAvailabilityStatus, UpdateAvailabilityStatus>();

            // ----- Interview Booking ---
            services.AddScoped<ICreateBookingCheckoutUrl, CreateBookingCheckoutUrl>();
            services.AddScoped<IHandldeInterviewBookingUpdate, HandldeInterviewBookingUpdate>();
            services.AddScoped<IGetInterviewBooking, GetInterviewBooking>();
            services.AddScoped<ICancelInterview, CancelInterview>();
            services.AddScoped<IPayoutForCoachAfterInterview, PayoutForCoachAfterInterview>();
            services.AddScoped<IGetInterviewBookingHistory, GetInterviewBookingHistory>();

            // ----- Coach & Candidate Details ---
            services.AddScoped<IGetCoachDetails, GetCoachDetails>();
            services.AddScoped<IGetCandidateDetails, GetCandidateDetails>();

            // ----- CandidateProfile ----
            services.AddScoped<ICreateCandidateProfile, CreateCandidateProfile>();
            services.AddScoped<IUpdateCandidateProfile, UpdateCandidateProfile>();
            services.AddScoped<IViewCandidateProfile, ViewCandidateProfile>();
            services.AddScoped<IDeleteCandidateProfile, DeleteCandidateProfile>();

            // ----- Reschedule Request ----
            services.AddScoped<ICreateRescheduleRequestUseCase, CreateRescheduleRequestUseCase>();
            services.AddScoped<IRespondToRescheduleRequestUseCase, RespondToRescheduleRequestUseCase>();
            services.AddScoped<IExpireRescheduleRequestsUseCase, ExpireRescheduleRequestsUseCase>();

            // ----- InterviewType ----
            services.AddScoped<IGetInterviewType, GetInterviewType>();
            services.AddScoped<IUpdateInterviewType, UpdateInterviewType>();
            services.AddScoped<ICreateInterviewType, CreateInterviewType>();
            services.AddScoped<IDeleteInterviewType, DeleteInterviewType>();

            // --- Interview Experience ---
            services.AddScoped<IGetInterviewExperiences, GetInterviewExperiences>();
            services.AddScoped<IGetInterviewExperienceDetail, GetInterviewExperienceDetail>();
            services.AddScoped<ICreateInterviewExperience, CreateInterviewExperience>();
            services.AddScoped<IUpdateInterviewExperience, UpdateInterviewExperience>();
            services.AddScoped<IDeleteInterviewExperience, DeleteInterviewExperience>();
            services.AddScoped<IAddQuestion, AddQuestion>();
            services.AddScoped<IUpdateQuestion, UpdateQuestion>();
            services.AddScoped<IDeleteQuestion, DeleteQuestion>();
            services.AddScoped<IGetQuestionList, GetQuestionList>();
            services.AddScoped<IGetQuestionDetail, GetQuestionDetail>();
            services.AddScoped<ISearchQuestions, SearchQuestions>();
            services.AddScoped<IReportQuestion, ReportQuestion>();
            services.AddScoped<IGetQuestionReports, GetQuestionReports>();
            services.AddScoped<IUpdateQuestionReportStatus, UpdateQuestionReportStatus>();

            // --- Comments ---
            services.AddScoped<IGetComments, GetComments>();
            services.AddScoped<IAddComment, AddComment>();
            services.AddScoped<IUpdateComment, UpdateComment>();
            services.AddScoped<IDeleteComment, DeleteComment>();

            // --- Likes & Saves ---
            services.AddScoped<ILikeQuestion, LikeQuestion>();
            services.AddScoped<ISaveQuestion, SaveQuestion>();
            services.AddScoped<IGetSavedQuestions, GetSavedQuestions>();
            services.AddScoped<ILikeComment, LikeComment>();

            // ----- CoachInterviewService ----
            services.AddScoped<CoachServiceInterfaces.ICreateCoachInterviewService, CoachServiceUseCases.CreateCoachInterviewService>();
            services.AddScoped<CoachServiceInterfaces.IUpdateCoachInterviewService, CoachServiceUseCases.UpdateCoachInterviewService>();
            services.AddScoped<CoachServiceInterfaces.IDeleteCoachInterviewService, CoachServiceUseCases.DeleteCoachInterviewService>();
            services.AddScoped<CoachServiceInterfaces.IGetCoachInterviewServices, CoachServiceUseCases.GetCoachInterviewServices>();

            // ----- BookingRequest ----
            services.AddScoped<BookingRequestInterfaces.ICreateJDBookingRequest, BookingRequestUseCases.CreateJDBookingRequest>();
            services.AddScoped<BookingRequestInterfaces.IRespondToBookingRequest, BookingRequestUseCases.RespondToBookingRequest>();
            services.AddScoped<BookingRequestInterfaces.IGetBookingRequests, BookingRequestUseCases.GetBookingRequests>();
            services.AddScoped<BookingRequestInterfaces.IGetBookingRequestDetail, BookingRequestUseCases.GetBookingRequestDetail>();
            services.AddScoped<BookingRequestInterfaces.IExpireBookingRequests, BookingRequestUseCases.ExpireBookingRequests>();
            services.AddScoped<BookingRequestInterfaces.IPayBookingRequest, BookingRequestUseCases.PayBookingRequest>();
            services.AddScoped<BookingRequestInterfaces.ICancelBookingRequest, BookingRequestUseCases.CancelBookingRequest>();
            
            // ----- Notification ----
            services.AddScoped<Interfaces.UseCases.Notification.INotificationUseCase, UseCases.Notification.NotificationUseCase>();

            // ----- SmartSearch ----
            services.AddScoped<Interfaces.UseCases.SmartSearch.ISyncCoachVectors, UseCases.SmartSearch.SyncCoachVectors>();
            services.AddScoped<Interfaces.UseCases.SmartSearch.ISyncQuestionVectors, UseCases.SmartSearch.SyncQuestionVectors>();
            services.AddScoped<Interfaces.UseCases.SmartSearch.ISmartSearchCoach, UseCases.SmartSearch.SmartSearchCoach>();
            services.AddScoped<Interfaces.UseCases.SmartSearch.ISmartSearchExtractDataFromFile, UseCases.SmartSearch.SmartSearchExtractDataFromFile>();
            services.AddScoped<Interfaces.UseCases.SmartSearch.ISmartSearchQuestion, UseCases.SmartSearch.SmartSearchQuestion>();

            return services;
        }

        public static IServiceCollection AddDomainBusinessRules(this IServiceCollection services)
        {
            services.AddScoped<IRefundPolicy, RefundPolicy>();

            return services;
        }
    }
}
