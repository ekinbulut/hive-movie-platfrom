using Infrastructure.Messaging.Configuration;
using Infrastructure.Messaging.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Messaging.Extensions;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddMessaging(this IServiceCollection services, string rabbitMqConn, string inputQueue, int workers = 4, int maxParallelism = 4)
    {
        services.ConfigureRebus(rabbitMqConn, inputQueue, workers, maxParallelism);
        services.AddScoped<IMessageBus, RebusBus>();
        return services;
    }
}