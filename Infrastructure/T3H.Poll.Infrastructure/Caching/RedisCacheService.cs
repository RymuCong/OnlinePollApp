using StackExchange.Redis;

namespace T3H.Poll.Infrastructure.Caching
{
    public class RedisCacheService
    {
        private readonly IDatabase _database;

        public RedisCacheService(IConnectionMultiplexer connection)
        {
            _database = connection.GetDatabase();
        }

        // Lưu cache
        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            var json = JsonSerializer.Serialize(value);
            await _database.StringSetAsync(key, json, expiry);
        }

        //  Lấy cache
        public async Task<T?> GetAsync<T>(string key)
        {
            var value = await _database.StringGetAsync(key);
            if (value.IsNullOrEmpty) return default;
            return JsonSerializer.Deserialize<T>(value!);
        }

        //  Kiểm tra tồn tại
        public async Task<bool> ExistsAsync(string key)
        {
            return await _database.KeyExistsAsync(key);
        }

        // 4 Xoá cache
        public async Task RemoveAsync(string key)
        {
            await _database.KeyDeleteAsync(key);
        }

        public async Task<T?> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null)
        {
            // 1. Kiểm tra cache
            var cachedData = await GetAsync<T>(key);
            if (cachedData != null)
                return cachedData;

            // 2. Nếu chưa có cache => gọi DB (factory)
            var data = await factory();
            if (data != null)
            {
                // 3. Lưu lại vào Redis
                await SetAsync(key, data, expiry);
            }

            return data;
        }
    }
}
