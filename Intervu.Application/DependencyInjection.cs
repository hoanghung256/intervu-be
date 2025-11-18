using Intervu.Application.Mappings;
using Intervu.Application.Services;
using Intervu.Application.UseCases.Authentication;
using Intervu.Application.Interfaces.UseCases.Authentication;
using Intervu.Application.Interfaces.UseCases.InterviewRoom;
using Intervu.Application.UseCases.InterviewRoom;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Intervu.Application.Interfaces.UseCases.Interviewer;
using Intervu.Application.UseCases.InterviewerProfile;
using Intervu.Application.Interfaces.UseCases.InterviewerProfile;
using CompanyInterfaces = Intervu.Application.Interfaces.UseCases.Company;
using CompanyUseCases = Intervu.Application.UseCases.Company;
using Intervu.Application.Interfaces.UseCases.Skill;
using Intervu.Application.UseCases.Skill;
using Intervu.Application.Interfaces.UseCases.Admin;
using AdminUseCases = Intervu.Application.UseCases.Admin;

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

            // Auth UseCases
            services.AddTransient<ILoginUseCase, LoginUseCase>();
            services.AddTransient<IRegisterUseCase, RegisterUseCase>();
            // ----- InterviewRoom ----
            services.AddScoped<ICreateInterviewRoom, CreateInterviewRoom>();
            services.AddScoped<IGetRoomHistory, GetRoomHistory>();
            // ----- InterviewerProfile ----
            services.AddScoped<ICreateInterviewProfile, CreateInterviewerProfile>();
            services.AddScoped<IUpdateInterviewProfile, UpdateInterviewerProfile>();
            services.AddScoped<IViewInterviewProfile, ViewInterviewerProfile>();
            services.AddScoped<IDeleteInterviewerProfile, DeleteInterviewerProfile>();
            services.AddScoped<IGetAllInterviewers, GetAllInterviewers>();
            services.AddScoped<CompanyInterfaces.IGetAllCompanies, CompanyUseCases.GetAllCompanies>();
            services.AddScoped<IGetAllSkills, GetAllSkills>();
            // ----- Admin ----
            services.AddScoped<IGetDashboardStats, AdminUseCases.GetDashboardStats>();
            services.AddScoped<IGetAllUsersForAdmin, AdminUseCases.GetAllUsers>();
            services.AddScoped<IGetAllCompaniesForAdmin, AdminUseCases.GetAllCompanies>();
            services.AddScoped<IGetAllPayments, AdminUseCases.GetAllPayments>();
            services.AddScoped<IGetAllFeedbacks, AdminUseCases.GetAllFeedbacks>();
            services.AddScoped<IGetAllInterviewersForAdmin, AdminUseCases.GetAllInterviewersForAdmin>();

            return services;
        }
    }
}
