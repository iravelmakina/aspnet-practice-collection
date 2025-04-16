using System.Net;
using System.Net.Sockets;
using StackExchange.Redis;

namespace DNET.Backend.Api.Infrastructure;

public class RateLimitMiddleware : IMiddleware
{
    private IConfiguration _configuration;
    private readonly IDatabase _db;


    public RateLimitMiddleware(IConfiguration configuration, IConnectionMultiplexer connectionMultiplexer)
    {
        _configuration = configuration;
        _db = connectionMultiplexer.GetDatabase();
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var maxRequestsPerMinute = Int32.Parse(_configuration["RateLimitSettings:RequestsPerMinute"]);
        var windowMinutes = Int32.Parse(_configuration["RateLimitSettings:WindowMinutes"]);
        
        var ip = context.Connection.RemoteIpAddress?.ToString();
        if (ip == "::1")  // to resolve localhost
        {
            var hostNames = Dns.GetHostEntry(ip);
            ip = hostNames.AddressList.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork).ToString();
        }
        
        var endpoint = context.Request.Path;
        var currentDatetime = DateTime.UtcNow;
        var currentWindow = new DateTime(currentDatetime.Year, currentDatetime.Month, currentDatetime.Day,
            currentDatetime.Hour, currentDatetime.Minute, 0, DateTimeKind.Utc).ToString("yyyy-MM-dd_HH-mm");
        
        var cacheKey = $"rateLimits:{endpoint}:{ip}:{currentWindow}";
        
        var cachedValue = await _db.StringIncrementAsync(cacheKey);
        _db.KeyExpireAsync(cacheKey, TimeSpan.FromMinutes(windowMinutes));
        
        if (cachedValue >= maxRequestsPerMinute)
        {
            context.Response.StatusCode = 429;
            
            await context.Response.WriteAsJsonAsync(new
            {
                Message = "Maximum number of requests exceed. Please try again later.",
                RetryAfterSeconds = windowMinutes * 60
            });
            return;
        }
        
        await next(context);
    }
}