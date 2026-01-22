namespace Auth_Redis.Services.Interfaces
{
    public interface IRedisService
    {
        Task SetAsync(string key, string value, TimeSpan expiry);
        Task<string> GetAsync(string key);
        Task DeleteAsync(string key);
    }
}
