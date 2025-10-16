using Domain.Interfaces;
using Infrastructure.Database.Context;
using Infrastructure.Database.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Hive.Idm.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDbContextInfra(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IConfigurationRepository, ConfigurationRepository>();
        
        services.AddDbContext<HiveDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
        });

        return services;
    }
}