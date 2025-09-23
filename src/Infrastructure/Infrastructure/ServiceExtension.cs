using System.Reflection;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class ServiceExtension
{
    public static IServiceCollection AddMediator(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<HiveDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
        });

        return services;
    }
}