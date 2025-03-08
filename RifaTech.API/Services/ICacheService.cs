using Microsoft.Extensions.Caching.Memory;

namespace RifaTech.API.Services
{
    public interface ICacheService
    {
        T GetOrCreate<T>(string key, Func<T> factory, TimeSpan? expiration = null);
        Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null);
        void Remove(string key);
    }

    public class MemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _cache;
        private readonly TimeSpan _defaultExpiration = TimeSpan.FromMinutes(5);

        public MemoryCacheService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public T GetOrCreate<T>(string key, Func<T> factory, TimeSpan? expiration = null)
        {
            if (_cache.TryGetValue(key, out T cachedItem))
            {
                return cachedItem;
            }

            var item = factory();

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(expiration ?? _defaultExpiration);

            _cache.Set(key, item, cacheEntryOptions);

            return item;
        }

        public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
        {
            if (_cache.TryGetValue(key, out T cachedItem))
            {
                return cachedItem;
            }

            var item = await factory();

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(expiration ?? _defaultExpiration);

            _cache.Set(key, item, cacheEntryOptions);

            return item;
        }

        public void Remove(string key)
        {
            _cache.Remove(key);
        }
    }
}
