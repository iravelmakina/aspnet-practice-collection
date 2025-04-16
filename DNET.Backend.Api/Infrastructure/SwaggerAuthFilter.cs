using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DNET.Backend.Api.Infrastructure;

public sealed class SwaggerAuthFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Parameters ??= new List<OpenApiParameter>();

        var allowAnonymousAttributes = context
            .MethodInfo
            .GetCustomAttributes<AllowAnonymousAttribute>();

        if (allowAnonymousAttributes.Any())
            return;

        var methodHeaders = context
            .MethodInfo
            .GetCustomAttributes<AuthorizeAttribute>();

        var controllerHeaders = context
            .MethodInfo
            .DeclaringType?
            .GetCustomAttributes<AuthorizeAttribute>() ?? [];

        var headers = controllerHeaders
            .Union(methodHeaders)
            .ToList();

        if (headers.Count == 0)
            return;

        operation.Security = new List<OpenApiSecurityRequirement>
        {
            new()
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new List<string>()
                }
            }
        };
    }
}
