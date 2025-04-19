namespace DNET.Backend.Api.Infrastructure;

public class LogMiddleware : IMiddleware
{
    private readonly Guid _id = Guid.NewGuid();
    private readonly ILogger<LogMiddleware> _logger;

    public LogMiddleware(ILogger<LogMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        _logger.LogInformation("Request {RequestId} {TraceIdentifier}: {Method} {Path}{QueryString} started at {Timestamp}",
            _id, context.TraceIdentifier, context.Request.Method, context.Request.Path, context.Request.QueryString, DateTime.Now);
        
        await next(context);
        
        _logger.LogInformation("Response {RequestId} {TraceIdentifier}: {Method} {Path}{QueryString} finished at {Timestamp} with status code {StatusCode}",
            _id, context.TraceIdentifier, context.Request.Method, context.Request.Path, context.Request.QueryString, DateTime.Now, context.Response.StatusCode);
    }
}
