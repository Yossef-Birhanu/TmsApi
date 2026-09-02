// using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TmsApi.Infrastructure.Persistence;

namespace TmsApi.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<TmsDb1Context>(options =>
        {
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"));

            options.LogTo(Console.WriteLine,LogLevel.Information);
             options.EnableSensitiveDataLogging();
        });

        return services;
    }
}