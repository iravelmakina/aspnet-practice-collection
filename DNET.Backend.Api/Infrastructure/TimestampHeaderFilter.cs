using Microsoft.AspNetCore.Mvc.Filters;

namespace DNET.Backend.Api.Infrastructure;

public class TimestampHeaderFilter : IResultFilter
{
    public void OnResultExecuting(ResultExecutingContext context)
    {
        context.HttpContext.Response.Headers.Append("X-Timestamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
    }

    public void OnResultExecuted(ResultExecutedContext context)
    {
        // Do nothing
    }
}