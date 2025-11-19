using Intervu.Application.Mappings;
using Intervu.Application.Services;
using Intervu.Application.UseCases.Authentication;
using Intervu.Application.Interfaces.UseCases.Authentication;
using Intervu.Application.Interfaces.UseCases.InterviewRoom;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Intervu.Application.UseCases.InterviewerProfile;
using Intervu.Application.Interfaces.UseCases.InterviewerProfile;
using Intervu.Application.Interfaces.UseCases.Company;
using Intervu.Application.UseCases.Company;
using Intervu.Application.Interfaces.UseCases.Skill;
using Intervu.Application.UseCases.Skill;
using Intervu.Application.Interfaces.UseCases.Availability;
using Intervu.Application.UseCases.Availability;
using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.Services.CodeGeneration;
using Intervu.Application.Interfaces.UseCases.InterviewRoom.InterviewRoom;
using Intervu.Application.Interfaces.UseCases.Email;
using Intervu.Application.UseCases.Email;

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
            services.AddSingleton<ICodeGenerationService, CSharpCodeGenerationService>();
            services.AddSingleton<ICodeGenerationService, JavaScriptCodeGenerationService>();
            services.AddSingleton<ICodeGenerationService, JavaCodeGenerationService>();

            // Auth UseCases
            services.AddTransient<ILoginUseCase, LoginUseCase>();
            services.AddTransient<IRegisterUseCase, RegisterUseCase>();
            // ----- InterviewRoom ----
            services.AddScoped<ICreateInterviewRoom, CreateInterviewRoom>();
            services.AddScoped<IGetRoomHistory, GetRoomHistory>();
            services.AddScoped<IUpdateRoom, UpdateRoom>();
            services.AddScoped<IGetCurrentRoom, GetCurrentRoom>();
            // ----- InterviewerProfile ----
            services.AddScoped<ICreateInterviewProfile, CreateInterviewerProfile>();
            services.AddScoped<IUpdateInterviewProfile, UpdateInterviewerProfile>();
            services.AddScoped<IViewInterviewProfile, ViewInterviewerProfile>();
            services.AddScoped<IDeleteInterviewerProfile, DeleteInterviewerProfile>();
            services.AddScoped<IGetAllInterviewers, GetAllInterviewers>();
            services.AddScoped<IGetAllCompanies, GetAllCompanies>();
            services.AddScoped<IGetAllSkills, GetAllSkills>();
            services.AddScoped<IGetInterviewerAvailabilities, GetInterviewerAvailabilities>();
            services.AddScoped<ICreateInterviewerAvailability, CreateInterviewerAvailability>();
            services.AddScoped<IDeleteInterviewerAvailability, DeleteInterviewerAvailability>();
            services.AddScoped<IUpdateInterviewerAvailability, UpdateInterviewerAvailability>();
            // ----- Email ----
            services.AddScoped<ISendBookingConfirmationEmail, SendBookingConfirmationEmail>();

            return services;
        }
    }
}
