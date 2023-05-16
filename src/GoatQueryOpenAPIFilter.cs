using System.Globalization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

// This will generate correct open API spec query parameters
// based off the [EnableQuery] attribute
public class GoatQueryOpenAPIFilter : IOperationFilter
{
    private readonly ISerializerDataContractResolver _serializerDataContractResolver;

    public GoatQueryOpenAPIFilter(ISerializerDataContractResolver serializerDataContractResolver)
    {
        _serializerDataContractResolver = serializerDataContractResolver;
    }

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.Parameters == null) operation.Parameters = new List<OpenApiParameter>();

        var descriptor = context.ApiDescription.ActionDescriptor as ControllerActionDescriptor;

        if (descriptor is null)
        {
            return;
        }

        var methodHasEnableQueryFilter = descriptor.MethodInfo.GetCustomAttributes(true).Any(x =>
        {
            var type = x.GetType();
            return type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(EnableQueryAttribute<>));
        });

        if (!methodHasEnableQueryFilter)
        {
            return;
        }

        typeof(Query).GetProperties().ToList().ForEach(x =>
        {
            var data = _serializerDataContractResolver.GetDataContractForType(x.PropertyType);

            operation.Parameters.Add(new OpenApiParameter()
            {
                Name = x.Name,
                In = ParameterLocation.Query,
                Required = false,
                Schema = new OpenApiSchema()
                {
                    // https://github.com/domaindrivendev/Swashbuckle.AspNetCore/blob/master/src/Swashbuckle.AspNetCore.SwaggerGen/SchemaGenerator/SchemaGenerator.cs#L272
                    Type = data.DataType.ToString().ToLower(CultureInfo.InvariantCulture),
                    Format = data.DataFormat
                }
            });
        });
    }
}