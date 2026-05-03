using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public sealed class EnableQueryTest : IAsyncLifetime
{
    private WebApplication _app = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();

        builder
            .Services.AddControllers()
            .AddApplicationPart(typeof(EnableQueryTestItemsController).Assembly)
            .ConfigureApplicationPartManager(manager =>
            {
                manager.FeatureProviders.Add(new EnableQueryTestControllerFeatureProvider());
            });
        builder.Services.AddSingleton<
            ISearchBinder<EnableQueryTestItem>,
            EnableQueryTestItemSearchBinder
        >();

        _app = builder.Build();
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
    public async Task Test_EnableQuery_ReturnsFilteredResults()
    {
        var response = await _client.GetAsync("/api/items?filter=name eq 'Alpha'");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<PagedResponse<EnableQueryTestItem>>(
            JsonOpts
        );
        Assert.NotNull(body);
        Assert.Single(body.Value);
        Assert.Equal("Alpha", body.Value.First().Name);
    }

    [Fact]
    public async Task Test_EnableQuery_ReturnsOrderedResults()
    {
        var response = await _client.GetAsync("/api/items?orderby=price desc");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<PagedResponse<EnableQueryTestItem>>(
            JsonOpts
        );
        Assert.NotNull(body);
        Assert.True(body.Value.Count() >= 2);
        Assert.Equal("Charlie", body.Value.First().Name); // Price 300 is highest
    }

    [Fact]
    public async Task Test_EnableQuery_TopAndSkip()
    {
        var response = await _client.GetAsync("/api/items?orderby=name asc&top=2&skip=1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<PagedResponse<EnableQueryTestItem>>(
            JsonOpts
        );
        Assert.NotNull(body);
        Assert.Equal(2, body.Value.Count());
        Assert.Equal("Beta", body.Value.First().Name); // Skipped Alpha
    }

    [Fact]
    public async Task Test_EnableQuery_Count()
    {
        var response = await _client.GetAsync("/api/items?count=true");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<PagedResponse<EnableQueryTestItem>>(
            JsonOpts
        );
        Assert.NotNull(body);
        Assert.NotNull(body.Count);
        Assert.Equal(3, body.Count);
    }

    [Fact]
    public async Task Test_EnableQuery_ExceedsMaxTop_ReturnsBadRequest()
    {
        var response = await _client.GetAsync("/api/items?top=999");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Test_EnableQuery_InvalidTop_ReturnsBadRequest()
    {
        var response = await _client.GetAsync("/api/items?top=abc");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Test_EnableQuery_InvalidSkip_ReturnsBadRequest()
    {
        var response = await _client.GetAsync("/api/items?skip=abc");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Test_EnableQuery_InvalidCount_ReturnsBadRequest()
    {
        var response = await _client.GetAsync("/api/items?count=maybe");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Test_EnableQuery_InvalidFilter_ReturnsBadRequest()
    {
        var response = await _client.GetAsync("/api/items?filter=nonExistentProp eq 'x'");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Test_EnableQuery_Search_WithSearchBinder()
    {
        var response = await _client.GetAsync("/api/items?search=alp");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<PagedResponse<EnableQueryTestItem>>(
            JsonOpts
        );
        Assert.NotNull(body);
        Assert.Single(body.Value);
        Assert.Equal("Alpha", body.Value.First().Name);
    }

    [Fact]
    public async Task Test_EnableQuery_CombinedFilterAndOrderBy()
    {
        var response = await _client.GetAsync(
            "/api/items?filter=price gt 50m&orderby=price asc&count=true"
        );

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<PagedResponse<EnableQueryTestItem>>(
            JsonOpts
        );
        Assert.NotNull(body);
        Assert.Equal(2, body.Count); // Beta (200) and Charlie (300)
        Assert.Equal("Beta", body.Value.First().Name);
    }

    [Fact]
    public async Task Test_EnableQuery_EmptyQuery_ReturnsAll()
    {
        var response = await _client.GetAsync("/api/items");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<PagedResponse<EnableQueryTestItem>>(
            JsonOpts
        );
        Assert.NotNull(body);
        Assert.Equal(3, body.Value.Count());
    }

    [Fact]
    public async Task Test_EnableQuery_DefaultMaxTop_AppliedWhenTopNotSet()
    {
        var response = await _client.GetAsync("/api/items");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<PagedResponse<EnableQueryTestItem>>(
            JsonOpts
        );
        Assert.NotNull(body);
        Assert.Equal(3, body.Value.Count());
    }

    private static readonly JsonSerializerOptions JsonOpts = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
    };
}

// --- Test infrastructure (top-level for controller discovery) ---

public record EnableQueryTestItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

public class EnableQueryTestItemSearchBinder : ISearchBinder<EnableQueryTestItem>
{
    public System.Linq.Expressions.Expression<Func<EnableQueryTestItem, bool>> Bind(
        string searchTerm
    )
    {
        var term = searchTerm.ToLower();
        return item => item.Name.ToLower().Contains(term);
    }
}

[ApiController]
[Route("api/items")]
public class EnableQueryTestItemsController : ControllerBase
{
    private static readonly List<EnableQueryTestItem> Items = new()
    {
        new EnableQueryTestItem
        {
            Id = 1,
            Name = "Alpha",
            Price = 10m,
        },
        new EnableQueryTestItem
        {
            Id = 2,
            Name = "Beta",
            Price = 200m,
        },
        new EnableQueryTestItem
        {
            Id = 3,
            Name = "Charlie",
            Price = 300m,
        },
    };

    [HttpGet]
    [EnableQuery<EnableQueryTestItem>(maxTop: 100)]
    public ActionResult<IEnumerable<EnableQueryTestItem>> Get()
    {
        return Ok(Items.AsQueryable());
    }
}

/// <summary>
/// Custom feature provider that registers the test controller explicitly,
/// since it lives in a test assembly that isn't discovered by default.
/// </summary>
public class EnableQueryTestControllerFeatureProvider : ControllerFeatureProvider
{
    protected override bool IsController(System.Reflection.TypeInfo typeInfo)
    {
        return typeInfo.AsType() == typeof(EnableQueryTestItemsController)
            || base.IsController(typeInfo);
    }
}
