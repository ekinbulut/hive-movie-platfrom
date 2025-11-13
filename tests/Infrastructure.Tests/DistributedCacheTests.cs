using Domain.Abstraction;
using Infrastructure.Caching;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Options;
using Testcontainers.Redis;
using Testcontainers.Xunit;
using Xunit.Abstractions;

namespace Infrastructure.Tests;

public class DistributedCacheTests(ITestOutputHelper testOutputHelper)
    : ContainerTest<RedisBuilder, RedisContainer>(testOutputHelper)
{
    private ICacheService _cache;

    protected override RedisBuilder Configure(RedisBuilder builder)
    {
        // 👇 Configure your container instance here.
        return builder.WithImage("redis:7.0");
    }
    
    [Fact]
    public async Task Test_SetAsync_GetAsync()
    {
        var options = Options.Create(new RedisCacheOptions
        {
            Configuration = Container.GetConnectionString()
        });
        var redisCache = new RedisCache(options);
        _cache = new CacheService(redisCache);
        
        var testKey = "test_key";
        var testValue = "test_value";
        await _cache.SetAsync(testKey, testValue);
        var retrievedValue = await _cache.GetAsync<string>(testKey);
        Assert.Equal(testValue, retrievedValue);
    }
}