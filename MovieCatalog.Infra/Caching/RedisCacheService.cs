using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using MovieCatalog.Application.Abstractions;
using StackExchange.Redis;

namespace MovieCatalog.Infra.Caching
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        private readonly IConnectionMultiplexer _redis;
        private readonly ILogger<RedisCacheService> _logger;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public RedisCacheService(IDistributedCache cache,
            IConnectionMultiplexer redis,
            ILogger<RedisCacheService> logger)
        {
            _cache = cache;
            _redis = redis;
            _logger = logger;
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            try
            {
                var data = await _cache.GetStringAsync(key);
                if (data is null) return default;

                return JsonSerializer.Deserialize<T>(data, JsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Redis GET failed for key {CacheKey}, falling back to source", key);
                return default;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            try
            {
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(5)
                };

                var data = JsonSerializer.Serialize(value, JsonOptions);
                await _cache.SetStringAsync(key, data, options);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Redis SET failed for key {CacheKey}", key);
            }
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                await _cache.RemoveAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Redis REMOVE failed for key {CacheKey}", key);
            }
        }

        public async Task RemoveByPrefixAsync(string prefix)
        {
            try
            {
                await _cache.RemoveAsync(prefix);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Redis REMOVE by prefix failed for {Prefix}", prefix);
            }
        }
    }
}
