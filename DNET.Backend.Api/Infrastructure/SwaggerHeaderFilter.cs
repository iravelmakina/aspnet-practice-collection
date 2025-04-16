using System.Reflection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DNET.Backend.Api.Infrastructure;

public sealed class SwaggerHeaderFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Parameters ??= new List<OpenApiParameter>();

        var methodHeaders = context
            .MethodInfo
            .GetCustomAttributes<SwaggerHeaderAttribute>();

        var controllerHeaders = context
            .MethodInfo
            .DeclaringType?
            .GetCustomAttributes<SwaggerHeaderAttribute>() ?? [];

        var headers = controllerHeaders
            .Union(methodHeaders)
            .ToList();

        if (!headers.Any())
            return;

        foreach (var header in headers)
        {
            var existingParam = operation.Parameters.FirstOrDefault(p => p.In == ParameterLocation.Header && p.Name == header.Name);
            if (existingParam != null)
                operation.Parameters.Remove(existingParam);

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = header.Name,
                In = ParameterLocation.Header,
                Description = header.Description,
                Required = header.Required,
                Schema = new OpenApiSchema
                {
                    Type = "string",
                    Format = header.Format
                }
            });
        }

    }
}
