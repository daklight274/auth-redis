using Auth_Redis.Services.Interfaces;
using StackExchange.Redis;

namespace Auth_Redis.Services
{
    public class RedisService : IRedisService
    {
        private readonly IDatabase _db;
        public RedisService(IConnectionMultiplexer redis)
        {
            _db = redis.GetDatabase();
        }

        public async Task SetAsync(string key, string value, TimeSpan expiry)
            => await _db.StringSetAsync(key, value, expiry);

        public async Task<string> GetAsync(string key)
            => await _db.StringGetAsync(key);

        public async Task DeleteAsync(string key)
            => await _db.KeyDeleteAsync(key);
    }
}
