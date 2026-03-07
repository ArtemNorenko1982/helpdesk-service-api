using HelpdeskService.Core.Interfaces;
using HelpdeskService.Data.Context;
using HelpdeskService.Data.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HelpdeskService.Data.Extensions;

public static class DataServiceExtensions
{
    public static IServiceCollection AddDataServices(this IServiceCollection services)
    {
        services.AddDbContext<HelpdeskDbContext>(options =>
            options.UseInMemoryDatabase("HelpdeskDb"));

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITicketsService, TicketsService>();

        return services;
    }
}
