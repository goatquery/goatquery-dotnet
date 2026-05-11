namespace GoatQuery.Tests;

using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public sealed class EnableQueryOperationTransformerTest : IAsyncLifetime
{
    private WebApplication _app = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();

        builder
            .Services.AddControllers()
            .AddApplicationPart(typeof(OpenApiTestItemsController).Assembly)
            .ConfigureApplicationPartManager(manager =>
            {
                manager.FeatureProviders.Add(new OpenApiTestControllerFeatureProvider());
            });

        builder.Services.AddOpenApi(options =>
        {
            options.AddOperationTransformer<EnableQueryOperationTransformer>();
        });

        _app = builder.Build();
        _app.MapOpenApi();
        _app.MapControllers();

        await _app.StartAsync();
        _client = _app.GetTestClient();
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _app.DisposeAsync();
    }

    [Fact]
    public async Task Test_Transformer_AddsParametersToEnableQueryEndpoint()
    {
        var doc = await GetOpenApiDocument();
        var parameters = doc
            .RootElement.GetProperty("paths")
            .GetProperty("/api/openapi-items")
            .GetProperty("get")
            .GetProperty("parameters");

        var paramNames = parameters
            .EnumerateArray()
            .Where(p => p.GetProperty("in").GetString() == "query")
            .Select(p => p.GetProperty("name").GetString())
            .ToHashSet();

        Assert.Contains("Filter", paramNames);
        Assert.Contains("OrderBy", paramNames);
        Assert.Contains("Top", paramNames);
        Assert.Contains("Skip", paramNames);
        Assert.Contains("Count", paramNames);
        Assert.Contains("Search", paramNames);
    }

    [Fact]
    public async Task Test_Transformer_SetsCorrectSchemaTypes()
    {
        var doc = await GetOpenApiDocument();
        var parameters = doc
            .RootElement.GetProperty("paths")
            .GetProperty("/api/openapi-items")
            .GetProperty("get")
            .GetProperty("parameters");

        var paramList = parameters
            .EnumerateArray()
            .Where(p => p.GetProperty("in").GetString() == "query")
            .ToDictionary(p => p.GetProperty("name").GetString()!, p => p);

        Assert.Equal(
            "string",
            paramList["Filter"].GetProperty("schema").GetProperty("type").GetString()
        );
        Assert.Equal(
            "string",
            paramList["OrderBy"].GetProperty("schema").GetProperty("type").GetString()
        );
        Assert.Equal(
            "integer",
            paramList["Top"].GetProperty("schema").GetProperty("type").GetString()
        );
        Assert.Equal(
            "integer",
            paramList["Skip"].GetProperty("schema").GetProperty("type").GetString()
        );
        Assert.Equal(
            "boolean",
            paramList["Count"].GetProperty("schema").GetProperty("type").GetString()
        );
        Assert.Equal(
            "string",
            paramList["Search"].GetProperty("schema").GetProperty("type").GetString()
        );
    }

    [Fact]
    public async Task Test_Transformer_AllParametersAreOptional()
    {
        var doc = await GetOpenApiDocument();
        var parameters = doc
            .RootElement.GetProperty("paths")
            .GetProperty("/api/openapi-items")
            .GetProperty("get")
            .GetProperty("parameters");

        var queryParams = parameters
            .EnumerateArray()
            .Where(p => p.GetProperty("in").GetString() == "query");

        foreach (var param in queryParams)
        {
            Assert.False(
                param.TryGetProperty("required", out var required) && required.GetBoolean(),
                $"Parameter '{param.GetProperty("name").GetString()}' should not be required"
            );
        }
    }

    [Fact]
    public async Task Test_Transformer_DoesNotAddParametersToEndpointWithoutEnableQuery()
    {
        var doc = await GetOpenApiDocument();
        var otherEndpoint = doc
            .RootElement.GetProperty("paths")
            .GetProperty("/api/openapi-other")
            .GetProperty("get");

        if (otherEndpoint.TryGetProperty("parameters", out var parameters))
        {
            var queryParams = parameters
                .EnumerateArray()
                .Where(p => p.GetProperty("in").GetString() == "query")
                .Select(p => p.GetProperty("name").GetString())
                .ToHashSet();

            Assert.DoesNotContain("Filter", queryParams);
            Assert.DoesNotContain("OrderBy", queryParams);
            Assert.DoesNotContain("Top", queryParams);
            Assert.DoesNotContain("Skip", queryParams);
            Assert.DoesNotContain("Count", queryParams);
            Assert.DoesNotContain("Search", queryParams);
        }
    }

    private async Task<JsonDocument> GetOpenApiDocument()
    {
        var response = await _client.GetAsync("/openapi/v1.json");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        return JsonDocument.Parse(json);
    }
}

public record OpenApiTestItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

[ApiController]
[Route("api/openapi-items")]
public class OpenApiTestItemsController : ControllerBase
{
    [HttpGet]
    [EnableQuery<OpenApiTestItem>(maxTop: 100)]
    public ActionResult<IEnumerable<OpenApiTestItem>> Get()
    {
        return Ok(Array.Empty<OpenApiTestItem>().AsQueryable());
    }
}

[ApiController]
[Route("api/openapi-other")]
public class OpenApiTestOtherController : ControllerBase
{
    [HttpGet]
    public ActionResult<string> Get()
    {
        return Ok("no query parameters");
    }
}

public class OpenApiTestControllerFeatureProvider : ControllerFeatureProvider
{
    protected override bool IsController(System.Reflection.TypeInfo typeInfo)
    {
        return typeInfo.AsType() == typeof(OpenApiTestItemsController)
            || typeInfo.AsType() == typeof(OpenApiTestOtherController)
            || base.IsController(typeInfo);
    }
}
