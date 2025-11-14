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
using Intervu.Application.Interfaces.UseCases.Company;
using Intervu.Application.UseCases.Company;

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
            services.AddScoped<IGetAllCompanies, GetAllCompanies>();

            return services;
        }
    }
}
