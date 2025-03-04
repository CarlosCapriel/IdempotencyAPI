using IdempotencyAPI.Application.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using RedLockNet;
using System.Text.Json;

namespace IdempotencyAPI.Application.Middleware
{
    public class IdempotencyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IDistributedCache _cache;
        private readonly IDistributedLockFactory _lockFactory;

        public IdempotencyMiddleware(RequestDelegate next, IDistributedCache cache, IDistributedLockFactory distributedLockFactory)
        {
            _next = next;
            _cache = cache;
            _lockFactory = distributedLockFactory;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Method != HttpMethod.Post.Method || !context.Request.Path.StartsWithSegments("/WeatherForecast"))
            {
                await _next(context);

                return;
            }

            if (!context.Request.Headers.TryGetValue("Idempotency-Key", out var idempotencyKey))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("Idempotency-Key header requerido");
                
                return;
            }
            
            var cacheKey = $"Idempotency_{idempotencyKey}";
            await using (var redLock = await _lockFactory.CreateLockAsync(
            cacheKey,
            TimeSpan.FromMinutes(5),
            TimeSpan.FromSeconds(5),
            TimeSpan.FromSeconds(1)))
            {
                if (!redLock.IsAcquired)
                {
                    context.Response.StatusCode = StatusCodes.Status409Conflict;
                    await context.Response.WriteAsync("Another request is processing it");

                    return;
                }
                var cacheResponse = await _cache.GetAsync(cacheKey);
                if (cacheResponse != null)
                {
                    Console.WriteLine("Obtuvo cache");
                    var response = JsonSerializer.Deserialize<IdempotentResponse>(cacheResponse);
                    context.Response.StatusCode = StatusCodes.Status200OK;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(response!.Body ?? string.Empty);
                    return;
                }
                // Si no existe, procesar la solicitud y almacenar respuesta
                var originalBodyStream = context.Response.Body;
                using var responseBody = new MemoryStream();
                context.Response.Body = responseBody;


                await _next(context);

                if (context.Response.StatusCode == StatusCodes.Status200OK)
                {
                    responseBody.Seek(0, SeekOrigin.Begin);
                    var body = await new StreamReader(responseBody).ReadToEndAsync();
                    var idempotentResponse = new IdempotentResponse
                    {
                        StatusCode = context.Response.StatusCode,
                        Body = body
                    };
                    Console.WriteLine("Actualiza cache");
                    await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(idempotentResponse),
                            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) });
                }

                responseBody.Seek(0, SeekOrigin.Begin);
                await responseBody.CopyToAsync(originalBodyStream);
            }

        }

    }
}
