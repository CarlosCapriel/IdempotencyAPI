using IdempotencyAPI.Application.IdempotencyService;
using IdempotencyAPI.Application.Middleware;
using RedLockNet;
using RedLockNet.SERedis.Configuration;
using RedLockNet.SERedis;
using StackExchange.Redis;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()));

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("RedisCacheUrl");
});
builder.Services.AddSingleton<IDistributedLockFactory>(provider =>
{
    var connectionMultiplexer = ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("RedisCacheUrl"));
    return RedLockFactory.Create(
        new List<RedLockMultiplexer> { connectionMultiplexer },
        new RedLockRetryConfiguration(
            retryCount: 3,
            retryDelayMs: 200
        )
    );
});
builder.Services.AddScoped<IIdempotencyService, IdempotencyService>();
var app = builder.Build();
app.UseMiddleware<IdempotencyMiddleware>();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
