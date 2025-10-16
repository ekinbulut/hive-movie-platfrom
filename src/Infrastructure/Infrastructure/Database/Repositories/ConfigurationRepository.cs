using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database.Repositories;

public class ConfigurationRepository(HiveDbContext context) : IConfigurationRepository
{
    public Task<Configuration?> GetConfigurationByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return context.Configurations.FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);
    }

    public Task<bool> AddConfigurationAsync(Configuration configuration, CancellationToken cancellationToken = default)
    {
        context.Configurations.Add(configuration);
        return context.SaveChangesAsync(cancellationToken).ContinueWith(t => t.Result > 0, cancellationToken);
    }

    public async Task<bool> UpdateConfigurationAsync(Configuration configuration, CancellationToken cancellationToken = default)
    {
        var exists = await context.Configurations.AnyAsync(c => c.Id == configuration.Id, cancellationToken);
    
        if (!exists)
            return false;

        configuration.UpdatedAt = DateTime.UtcNow;
        context.Configurations.Update(configuration);
        return await context.SaveChangesAsync(cancellationToken) > 0;
    }
}