using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Caching.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDistributedCaching(this IServiceCollection services, IConfigurationManager configuration)
    {
        var useRedis = configuration.GetConnectionString("Redis") 
                       ?? Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING") 
                       ?? "localhost:6379";
        
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = useRedis;
            options.InstanceName = "SampleInstance";
        });

        return services;
    }
}