#if NET9_0_OR_GREATER

namespace GoatQuery;

using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

/// <summary>
/// OpenAPI operation transformer that adds GoatQuery query parameters
/// to any endpoint decorated with <see cref="EnableQueryAttribute{T}"/>.
/// Parameter names and types match what ASP.NET generates for minimal API
/// endpoints using <c>[AsParameters] Query</c>.
/// <para>
/// Register with:
/// <code>
/// builder.Services.AddOpenApi(options =&gt;
/// {
///     options.AddOperationTransformer&lt;EnableQueryOperationTransformer&gt;();
/// });
/// </code>
/// </para>
/// </summary>
public sealed class EnableQueryOperationTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken
    )
    {
        var hasEnableQuery = context.Description.ActionDescriptor.EndpointMetadata.Any(m =>
        {
            var type = m.GetType();
            return type.IsGenericType
                && type.GetGenericTypeDefinition() == typeof(EnableQueryAttribute<>);
        });

        if (!hasEnableQuery)
            return Task.CompletedTask;

        operation.Parameters ??= new List<OpenApiParameter>();

        operation.Parameters.Add(
            new OpenApiParameter
            {
                Name = "Top",
                In = ParameterLocation.Query,
                Required = false,
                Schema = new OpenApiSchema { Type = "integer", Format = "int32" },
            }
        );

        operation.Parameters.Add(
            new OpenApiParameter
            {
                Name = "Skip",
                In = ParameterLocation.Query,
                Required = false,
                Schema = new OpenApiSchema { Type = "integer", Format = "int32" },
            }
        );

        operation.Parameters.Add(
            new OpenApiParameter
            {
                Name = "Count",
                In = ParameterLocation.Query,
                Required = false,
                Schema = new OpenApiSchema { Type = "boolean" },
            }
        );

        operation.Parameters.Add(
            new OpenApiParameter
            {
                Name = "OrderBy",
                In = ParameterLocation.Query,
                Required = false,
                Schema = new OpenApiSchema { Type = "string" },
            }
        );

        operation.Parameters.Add(
            new OpenApiParameter
            {
                Name = "Search",
                In = ParameterLocation.Query,
                Required = false,
                Schema = new OpenApiSchema { Type = "string" },
            }
        );

        operation.Parameters.Add(
            new OpenApiParameter
            {
                Name = "Filter",
                In = ParameterLocation.Query,
                Required = false,
                Schema = new OpenApiSchema { Type = "string" },
            }
        );

        return Task.CompletedTask;
    }
}

#endif
