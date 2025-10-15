using Domain.Entities;

namespace Domain.Interfaces;

public interface IConfigurationRepository
{
    Task<Configuration?> GetConfigurationByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> AddConfigurationAsync(Configuration configuration, CancellationToken cancellationToken = default);
    Task<bool> UpdateConfigurationAsync(Configuration configuration, CancellationToken cancellationToken = default);
}