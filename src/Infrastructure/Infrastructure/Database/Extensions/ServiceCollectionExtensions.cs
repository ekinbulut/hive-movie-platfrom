using Domain.Interfaces;
using Infrastructure.Database.Context;
using Infrastructure.Database.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Database.Extensions;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddMediator(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IMovieRepository, MovieRepository>();
        
        services.AddDbContext<HiveDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
        });

        return services;
    }
}