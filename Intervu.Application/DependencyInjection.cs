using Intervu.Application.Mappings;
using Intervu.Application.Services;
using Intervu.Application.UseCases.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Intervu.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddUseCases(this IServiceCollection services, IConfiguration configuration)
        {
            // Example of adding a service
            //builder.Services.AddTransient<IUserService, UserService>();
            services.AddAutoMapper(typeof(DependencyInjection));

            // Register Services
            // services.AddScoped<JwtService>();
            // services.AddScoped<PasswordHashHandler>();

            // Auth UseCases
            // services.AddTransient<ILoginUseCase, LoginUseCase>();
            // services.AddTransient<IRegisterUseCase, RegisterUseCase>();

            return services;
        }
    }
}
