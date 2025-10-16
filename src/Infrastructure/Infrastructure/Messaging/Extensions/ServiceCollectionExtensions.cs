using Infrastructure.Messaging.Configuration;
using Infrastructure.Messaging.Services;
using Microsoft.Extensions.DependencyInjection;
using Rebus.Routing.TypeBased;

namespace Infrastructure.Messaging.Extensions;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddMessaging(this IServiceCollection services, string rabbitMqConn, string inputQueue, int workers = 4, int maxParallelism = 4)
    {
        services.ConfigureRebus(rabbitMqConn, inputQueue, workers, maxParallelism);
        services.AddScoped<IMessageBus, RebusBus>();
        return services;
    }

    /// <summary>
    /// Add messaging with routing configuration for multiple queues
    /// </summary>
    public static IServiceCollection AddMessagingWithRouting(
        this IServiceCollection services, 
        string rabbitMqConn, 
        string inputQueue, 
        Action<TypeBasedRouterConfigurationExtensions.TypeBasedRouterConfigurationBuilder> configureRouting,
        int workers = 4, 
        int maxParallelism = 4)
    {
        services.ConfigureRebusWithRouting(rabbitMqConn, inputQueue, configureRouting, workers, maxParallelism);
        services.AddScoped<IMessageBus, RebusBus>();
        return services;
    }
}