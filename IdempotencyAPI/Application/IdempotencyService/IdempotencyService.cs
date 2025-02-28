
using IdempotencyAPI.Application.Model;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace IdempotencyAPI.Application.IdempotencyService
{
    public class IdempotencyService : IIdempotencyService
    {
        private readonly IDistributedCache _cache;
        public IdempotencyService(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task<T> HasProcessedAsync<T>(string idempotencyKey)
        {
            var result = await _cache.GetAsync(idempotencyKey);
            if (result == null)
            {
                return default;
            }

            var response = JsonSerializer.Deserialize<T>(result);

            return response;
        }

        public async Task MarkAsProcessedAsync(string cacheKey, string result)
        {
            var idempotentResponse = new IdempotentResponse
            {
                StatusCode = StatusCodes.Status200OK,
                Body = result
            };

            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(idempotentResponse),
                    new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) });
        }
    }
}
