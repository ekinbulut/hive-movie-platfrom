using System.Reflection;
using Domain.Abstraction.Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace Domain.Extension;

// write a dependency injection extension method to register the mediator and all command handlers in the assembly

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMediator(this IServiceCollection services, Assembly assembly)
    {
        services.AddSingleton<IMediator, Mediator>();

        var commandHandlerTypes = assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .SelectMany(t => t.GetInterfaces(), (t, i) => new { Type = t, Interface = i })
            .Where(ti => ti.Interface.IsGenericType &&
                         (ti.Interface.GetGenericTypeDefinition() == typeof(ICommandHandler<>) ||
                          ti.Interface.GetGenericTypeDefinition() == typeof(ICommandHandler<,>)))
            .Select(ti => new { HandlerType = ti.Type, ServiceType = ti.Interface });

        foreach (var handler in commandHandlerTypes)
        {
            services.AddTransient(handler.ServiceType, handler.HandlerType);
        }

        return services;
    }
}