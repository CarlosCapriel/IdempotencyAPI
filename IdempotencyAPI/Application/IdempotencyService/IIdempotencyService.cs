namespace IdempotencyAPI.Application.IdempotencyService
{
    public interface IIdempotencyService
    {
        Task<T> HasProcessedAsync<T>(string idempotencyKey);
        Task MarkAsProcessedAsync(string cacheKey, string result);
    }
}
