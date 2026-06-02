using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace System.Api.Filters;

public class ApiKeyOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var hasApiKeyAttribute = context.MethodInfo.DeclaringType?.GetCustomAttributes(true).OfType<ApiKeyAttribute>().Any() == true ||
                                 context.MethodInfo.GetCustomAttributes(true).OfType<ApiKeyAttribute>().Any() == true;

        if (!hasApiKeyAttribute)
            return;

        operation.Parameters ??= new List<OpenApiParameter>();

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "X-Api-Key",
            In = ParameterLocation.Header,
            Description = "API Key requerida para esta operación.",
            Required = true,
            Schema = new OpenApiSchema
            {
                Type = "string"
            }
        });
    }
}
