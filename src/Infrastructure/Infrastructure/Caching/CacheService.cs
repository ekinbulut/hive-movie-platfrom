using Domain.Abstraction;
using Microsoft.Extensions.Caching.Distributed;

namespace Infrastructure.Caching;

public class CacheService(
    IDistributedCache distributedCache) : ICacheService
{

    public Task<T?> GetAsync<T>(string key, CancellationToken token = default)
    {
        var value = distributedCache.GetStringAsync(key,token);
        return value.ContinueWith(t => t.Result == null ? default(T) : System.Text.Json.JsonSerializer.Deserialize<T>(t.Result), token);
        
    }
    
    public Task SetAsync<T>(string key, T value, CancellationToken token = default)
    {
        var serialized = System.Text.Json.JsonSerializer.Serialize(value);
        return distributedCache.SetStringAsync(key, serialized, token);
    }
}