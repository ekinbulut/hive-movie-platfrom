using Infrastructure.Messaging.Pipeline;
using Microsoft.Extensions.DependencyInjection;
using Rebus.Config;
using Rebus.Logging;
using Rebus.Pipeline;
using Rebus.Pipeline.Send;
using Rebus.Retry.Simple;
using Rebus.Routing.TypeBased;

namespace Infrastructure.Messaging.Configuration;

public static class RebusConfiguration
{
    public static void ConfigureRebus(
        this IServiceCollection services,
        string amqpConnectionString,
        string inputQueue,
        int workers = 2,
        int maxParallelism = 4)
    {
        services.AddRebus(configure => configure
            .Logging(l => l.Use(new NullLoggerFactory()))
            .Transport(t => t.UseRabbitMq(amqpConnectionString, inputQueue))
            .Options(o =>
            {
                o.SetNumberOfWorkers(workers);
                o.SetMaxParallelism(maxParallelism);

                
                // Simple 2nd level retry then moves to error queue
                o.RetryStrategy(
                    errorQueueName: $"{inputQueue}-error",
                    maxDeliveryAttempts: 5,
                    secondLevelRetriesEnabled: true);
                
                o.Decorate<IPipeline>(c =>
                {
                    var pipeline = c.Get<IPipeline>();
                    return new PipelineStepInjector(pipeline)
                        .OnSend(new CorrelationOutgoingStep(),
                            PipelineRelativePosition.After,
                            typeof(SendOutgoingMessageStep));
                });
            })
        );
    }

    /// <summary>
    /// Configure Rebus with routing for multiple queues
    /// </summary>
    public static void ConfigureRebusWithRouting(
        this IServiceCollection services,
        string amqpConnectionString,
        string inputQueue,
        Action<TypeBasedRouterConfigurationExtensions.TypeBasedRouterConfigurationBuilder> configureRouting,
        int workers = 2,
        int maxParallelism = 4)
    {
        services.AddRebus(configure => configure
            .Logging(l => l.Use(new NullLoggerFactory()))
            .Transport(t => t.UseRabbitMq(amqpConnectionString, inputQueue))
            .Routing(r => configureRouting(r.TypeBased()))
            .Options(o =>
            {
                o.SetNumberOfWorkers(workers);
                o.SetMaxParallelism(maxParallelism);

                // Simple 2nd level retry then moves to error queue
                o.RetryStrategy(
                    errorQueueName: $"{inputQueue}-error",
                    maxDeliveryAttempts: 5,
                    secondLevelRetriesEnabled: true);
                
                o.Decorate<IPipeline>(c =>
                {
                    var pipeline = c.Get<IPipeline>();
                    return new PipelineStepInjector(pipeline)
                        .OnSend(new CorrelationOutgoingStep(),
                            PipelineRelativePosition.After,
                            typeof(SendOutgoingMessageStep));
                });
            })
        );
    }
}