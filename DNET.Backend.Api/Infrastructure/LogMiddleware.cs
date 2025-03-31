namespace DNET.Backend.Api.Infrastructure;

public class LogMiddleware : IMiddleware
{
    private readonly Guid _id = Guid.NewGuid();

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        Console.WriteLine($"Request {_id} {context.TraceIdentifier}: {context.Request.Method} {context.Request.Path} {context.Request.QueryString} Started at {DateTime.Now}");
        await next(context);
        Console.WriteLine($"Response {_id} {context.TraceIdentifier}: {context.Request.Method} {context.Request.Path} {context.Request.QueryString} Finished at {DateTime.Now} with status code {context.Response.StatusCode}");
    }
}
