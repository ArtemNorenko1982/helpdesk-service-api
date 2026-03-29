using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using HelpdeskService.Data.Context;

namespace HelpdeskService.Data.Extensions
{
    public static class HealthCheckServiceExtensions
    {
        public static IServiceCollection AddApplicationHealthChecks(this IServiceCollection services)
        {
            services.AddHealthChecks()
                .AddDbContextCheck<HelpdeskDbContext>(name: "database", tags: new[] { "ready" })
                .AddCheck("self", ()=> HealthCheckResult.Healthy(), tags: new[] { "live" });

            return services;
        }
    }
}
