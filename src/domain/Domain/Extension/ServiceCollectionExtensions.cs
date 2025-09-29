using System.Reflection;
using Domain.Abstraction.Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace Domain.Extension;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMediator(this IServiceCollection services, params Assembly[] assemblies)
    {
        services.AddScoped<IMediator, Mediator>();

        // Get all assemblies if none provided
        var assembliesToScan = assemblies.Any() 
            ? assemblies 
            : new[] { Assembly.GetExecutingAssembly(), Assembly.GetCallingAssembly(), Assembly.GetEntryAssembly() }
                .Where(a => a != null)
                .Concat(AppDomain.CurrentDomain.GetAssemblies())
                .Distinct()
                .ToArray();

        foreach (var assembly in assembliesToScan)
        {
            RegisterHandlersFromAssembly(services, assembly);
        }

        return services;
    }
    
    private static void RegisterHandlersFromAssembly(IServiceCollection services, Assembly assembly)
    {
        var handlerTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && !t.IsInterface)
            .Where(t => t.GetInterfaces().Any(i =>
                i.IsGenericType && (
                    i.GetGenericTypeDefinition() == typeof(ICommandHandler<,>) ||
                    i.GetGenericTypeDefinition() == typeof(ICommandHandler<>) ||
                    i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>)
                )))
            .ToList();

        foreach (var handlerType in handlerTypes)
        {
            var interfaces = handlerType.GetInterfaces()
                .Where(i => i.IsGenericType && (
                    i.GetGenericTypeDefinition() == typeof(ICommandHandler<,>) ||
                    i.GetGenericTypeDefinition() == typeof(ICommandHandler<>) ||
                    i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>)
                ));

            foreach (var @interface in interfaces)
            {
                services.AddScoped(@interface, handlerType);
            }
        }
    }

}
