using System.Text.Json;
using Domain.Entities;
using Domain.Interfaces;
using Hive.Idm.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Hive.Idm.Infrastructure.Repositories;

public class ConfigurationRepository(IdmDbContext context) : IConfigurationRepository
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

    public Task<bool> UpdateConfigurationAsync(Configuration configuration, CancellationToken cancellationToken = default)
    {
        context.Configurations.Update(configuration);
        return context.SaveChangesAsync(cancellationToken).ContinueWith(t => t.Result > 0, cancellationToken);
    }
}