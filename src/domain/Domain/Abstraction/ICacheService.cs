namespace Domain.Abstraction;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken token = default);
    Task SetAsync<T>(string key, T value, CancellationToken token = default);
}