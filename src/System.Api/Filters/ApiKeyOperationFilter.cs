namespace System.Api.Filters;

// using Microsoft.AspNetCore.OpenApi;
// using Microsoft.OpenApi.Models;
// using System.Reflection;
//
// namespace System.Api.Filters;
//
// // 1. Cambiamos de IOperationFilter a IOperationTransformer
// public class ApiKeyOperationTransformer : IOperationTransformer
// {
//     // Reemplazamos el método Apply por TransformAsync
//     public Task TransformAsync(OpenApiOperation operation, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
//     {
//         // En .NET 9 obtenemos el MethodInfo a través del contexto de OpenAPI
//         var methodInfo = context.Description.ActionDescriptor.EndpointMetadata
//             .OfType<MethodInfo>()
//             .FirstOrDefault();
//
//         if (methodInfo == null)
//             return Task.CompletedTask;
//
//         // Mantenemos tu misma lógica de búsqueda del atributo [ApiKeyAttribute]
//         var hasApiKeyAttribute = methodInfo.DeclaringType?.GetCustomAttributes(true).OfType<ApiKeyAttribute>().Any() == true ||
//                                  methodInfo.GetCustomAttributes(true).OfType<ApiKeyAttribute>().Any() == true;
//
//         if (!hasApiKeyAttribute)
//             return Task.CompletedTask;
//
//         operation.Parameters ??= new List<OpenApiParameter>();
//
//         operation.Parameters.Add(new OpenApiParameter
//         {
//             Name = "X-Api-Key",
//             In = ParameterLocation.Header,
//             Description = "API Key requerida para esta operación.",
//             Required = true,
//             Schema = new OpenApiSchema
//             {
//                 Type = "string"
//             }
//         });
//
//         return Task.CompletedTask;
//     }
// }
//
// // Atributo dummy por si necesitas que compile también el atributo temporalmente
// [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
// public class ApiKeyAttribute : Attribute { }