using Microsoft.Extensions.Caching.Memory;
using MovieInformationProvider.Application.Common.Caching;

namespace MovieInformationProvider.Infrastructure.Caching;

public sealed class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;

    public MemoryCacheService(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public bool TryGetValue<T>(string key, out T? value)
    {
        return _memoryCache.TryGetValue(key, out value);
    }

    public void Set<T>(string key, T value, TimeSpan expiration)
    {
        _memoryCache.Set(key, value, expiration);
    }
}