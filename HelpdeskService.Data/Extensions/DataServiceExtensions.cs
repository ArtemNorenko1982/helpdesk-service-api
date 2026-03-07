using HelpdeskService.Core.Interfaces;
using HelpdeskService.Data.Context;
using HelpdeskService.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HelpdeskService.Data.Extensions;

public static class DataServiceExtensions
{
    public static IServiceCollection AddDataServices(this IServiceCollection services)
    {
        services.AddDbContext<HelpdeskDbContext>(options =>
            options.UseLazyLoadingProxies()
                   .UseInMemoryDatabase("HelpdeskDb"));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITicketRepository, TicketRepository>();

        return services;
    }
}
