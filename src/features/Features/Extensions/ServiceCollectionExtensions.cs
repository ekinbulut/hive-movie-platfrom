using Domain.Extension;
using FastEndpoints;
using Microsoft.Extensions.DependencyInjection;

namespace Features.Extensions;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddFastApiRoutes(this IServiceCollection services)
    {
        services.AddFastEndpoints();
        return services;
    }
}