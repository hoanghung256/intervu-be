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
using Intervu.Application.UseCases.InterviewBooking;
using Intervu.Application.UseCases.InterviewRoom;
using Intervu.Application.Utils;
using Intervu.Application.UseCases.Admin;
using Intervu.Application.Interfaces.UseCases.PasswordReset;
using Intervu.Application.UseCases.PasswordReset;
using Intervu.Application.UseCases.Candidate;
using Intervu.Application.UseCases.CandidateProfile;

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
            // ----- CoachProfile ----
            services.AddScoped<ICreateCoachProfile, CreateCoachProfile>();
            services.AddScoped<IUpdateCoachProfile, UpdateCoachProfile>();
            services.AddScoped<IViewCoachProfile, ViewCoachProfile>();
            services.AddScoped<IDeleteCoachProfile, DeleteCoachProfile>();
            services.AddScoped<IGetAllCoach, GetAllCoach>();
            services.AddScoped<CompanyInterfaces.IGetAllCompanies, CompanyUseCases.GetAllCompanies>();
            services.AddScoped<IGetAllSkills, GetAllSkills>();
            // ----- Admin ----
            services.AddScoped<IGetDashboardStats, AdminUseCases.GetDashboardStats>();
            services.AddScoped<IGetAllUsersForAdmin, AdminUseCases.GetAllUsers>();
            services.AddScoped<IGetAllCompaniesForAdmin, AdminUseCases.GetAllCompanies>();
            services.AddScoped<IGetAllPayments, AdminUseCases.GetAllPayments>();
            services.AddScoped<IGetAllFeedbacks, AdminUseCases.GetAllFeedbacks>();
            services.AddScoped<IGetAllCoachForAdmin, AdminUseCases.GetAllCoachForAdmin>();
            // ----- Feedback ----
            services.AddScoped<IGetFeedbacks, GetFeedbacks>();
            services.AddScoped<ICreateFeedback, CreateFeedback>();
            services.AddScoped<IUpdateFeedback, UpdateFeedback>();

            // ----- Coach Availability ----
            services.AddScoped<IGetCoachAvailabilities, GetCoachAvailabilities>();
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
            services.AddScoped<IUpdateBookingStatus, UpdateBookingStatus>();
            services.AddScoped<IGetInterviewBooking, GetInterviewBooking>();
            services.AddScoped<IPayoutForCoachAfterInterview, PayoutForCoachAfterInterview>();

            // ----- Coach & Candidate Details ---
            services.AddScoped<IGetCoachDetails, GetCoachDetails>();
            services.AddScoped<IGetCandidateDetails, GetCandidateDetails>();

            // ----- CandidateProfile ----
            services.AddScoped<ICreateCandidateProfile, CreateCandidateProfile>();
            services.AddScoped<IUpdateCandidateProfile, UpdateCandidateProfile>();
            services.AddScoped<IViewCandidateProfile, ViewCandidateProfile>();
            services.AddScoped<IDeleteCandidateProfile, DeleteCandidateProfile>();

            return services;
        }
    }
}
