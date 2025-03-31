using DNET.Backend.Api.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace DNET.Backend.Api.Infrastructure;

public class AuthorizationFilter : IAuthorizationFilter
{
    private IOptionsSnapshot<AuthorizarionOptions> _options;

    public AuthorizationFilter(IOptionsSnapshot<AuthorizarionOptions> options)
    {
        _options = options;
    }
    
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        if (context.HttpContext.Request.Headers.TryGetValue("X-API-KEY", out var apiKey))
        {
            if (string.IsNullOrEmpty(apiKey) || !_options.Value.ApiKeys.ContainsKey(apiKey))
                SetInvalidResult(context);

            return;
        }
        
        SetInvalidResult(context);

    }
    
    private void SetInvalidResult(AuthorizationFilterContext context)
    {
        context.Result = new ObjectResult(new { message = "Unathorized", statusCode = 401 }) 
        {
            StatusCode = 401
        };
    }
}