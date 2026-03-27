using HelpdeskService.Core.Interfaces;
using HelpdeskService.Data.Context;
using HelpdeskService.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HelpdeskService.Data.Extensions;

public static class DataServiceExtensions
{
    public static IServiceCollection AddDataServices(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITicketRepository, TicketRepository>();

        return services;
    }

    public static IServiceCollection AddSqliteDataBase(this IServiceCollection services)
    {
        services.AddDbContext<HelpdeskDbContext>(options =>
            options.UseLazyLoadingProxies()
            .UseSqlite($"Data Source={AppDomain.CurrentDomain.BaseDirectory}HelpDesk.db"));
        
        return services;
    }

    public static IServiceCollection AddDataBase(this IServiceCollection services)
    {
        var serviceProvider = services.BuildServiceProvider();
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<HelpdeskDbContext>(options =>
            options.UseLazyLoadingProxies()
            .UseSqlServer(connectionString));

        return services;
    }
}
