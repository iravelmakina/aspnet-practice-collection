using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text;

public class ETagFilter : IResultFilter
{
    public void OnResultExecuting(ResultExecutingContext context)
    {
        if (context.Result is ObjectResult objectResult && context.HttpContext.Request.Method == "GET")
        {
            var json = System.Text.Json.JsonSerializer.Serialize(objectResult.Value);
            var bytes = Encoding.UTF8.GetBytes(json);
            var hash = System.Security.Cryptography.SHA256.HashData(bytes);
            var eTag = Convert.ToBase64String(hash);
            
            if (context.HttpContext.Request.Headers.TryGetValue("If-None-Match", out var value) && value.ToString() == eTag)
            {
                context.Result = new StatusCodeResult(304);
            }
            
            context.HttpContext.Response.Headers.ETag = eTag;
        }
    }

    public void OnResultExecuted(ResultExecutedContext context) { }
}
