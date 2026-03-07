using HelpdeskService.Core.Interfaces;
using HelpdeskService.Data.Services;
using Microsoft.Extensions.DependencyInjection;

namespace HelpdeskService.Data.Extensions;

public static class BusinessServiceExtensions
{
    public static IServiceCollection AddBusinessServices(this IServiceCollection services)
    {
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<ITicketMapper, TicketMapper>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITicketsService, TicketsService>();

        return services;
    }
}
