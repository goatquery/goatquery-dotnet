namespace GoatQuery;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

/// <summary>
/// An action filter that automatically applies GoatQuery filtering, ordering, pagination,
/// and search to an <see cref="IQueryable{T}"/> returned from a controller action.
/// <para>
/// The action must return an <see cref="ObjectResult"/> whose value is <see cref="IQueryable{T}"/>.
/// Query parameters (<c>filter</c>, <c>orderby</c>, <c>top</c>, <c>skip</c>, <c>count</c>, <c>search</c>)
/// are read from the HTTP request query string.
/// </para>
/// <para>
/// The <see cref="JsonNamingPolicy"/> is automatically resolved from <c>IOptions&lt;JsonOptions&gt;</c>
/// in DI when not explicitly configured, so property names in query strings match your
/// configured JSON serialization policy.
/// </para>
/// </summary>
/// <typeparam name="T">The entity type exposed by the action.</typeparam>
public sealed class EnableQueryAttribute<T> : ActionFilterAttribute
{
    private readonly QueryOptions? _options;

    public EnableQueryAttribute(int maxTop, int maxPropertyMappingDepth = 5)
    {
        var options = new QueryOptions()
        {
            MaxTop = maxTop,
            MaxPropertyMappingDepth = maxPropertyMappingDepth,
        };

        _options = options;
    }

    public EnableQueryAttribute() { }

    public override void OnActionExecuted(ActionExecutedContext context)
    {
        var result = context.Result as ObjectResult;
        if (result is null)
            return;

        var queryable = result.Value as IQueryable<T>;
        if (queryable is null)
            return;

        var queryString = context.HttpContext.Request.Query;

        // Top
        queryString.TryGetValue("top", out var topQuery);

        if (!int.TryParse(topQuery.ToString(), out int top) && !string.IsNullOrEmpty(topQuery))
        {
            context.Result = new BadRequestObjectResult(
                new { message = "The query parameter 'Top' could not be parsed to an integer" }
            );
            return;
        }

        // Skip
        queryString.TryGetValue("skip", out var skipQuery);
        var skipString = skipQuery.ToString();

        if (!int.TryParse(skipString, out int skip) && !string.IsNullOrEmpty(skipQuery))
        {
            context.Result = new BadRequestObjectResult(
                new { message = "The query parameter 'Skip' could not be parsed to an integer" }
            );
            return;
        }

        // Count
        queryString.TryGetValue("count", out var countQuery);
        var countString = countQuery.ToString();

        if (!bool.TryParse(countString, out bool count) && !string.IsNullOrEmpty(countString))
        {
            context.Result = new BadRequestObjectResult(
                new { message = "The query parameter 'Count' could not be parsed to a boolean" }
            );
            return;
        }

        // Order by
        queryString.TryGetValue("orderby", out var orderbyQuery);

        // Search
        queryString.TryGetValue("search", out var searchQuery);
        var search = searchQuery.ToString();

        // Filter
        queryString.TryGetValue("filter", out var filterQuery);

        var query = new Query()
        {
            Top = string.IsNullOrEmpty(topQuery.ToString()) ? null : top,
            Skip = string.IsNullOrEmpty(skipString) ? null : skip,
            Count = string.IsNullOrEmpty(countString) ? null : count,
            OrderBy = orderbyQuery.ToString(),
            Search = search,
            Filter = filterQuery.ToString(),
        };

        ISearchBinder<T>? searchBinder = null;

        if (!string.IsNullOrEmpty(search))
        {
            searchBinder =
                context.HttpContext.RequestServices.GetService(typeof(ISearchBinder<T>))
                as ISearchBinder<T>;
        }

        var applyOptions = _options is not null
            ? new QueryOptions
            {
                MaxTop = _options.MaxTop,
                MaxPropertyMappingDepth = _options.MaxPropertyMappingDepth,
                PropertyNamingPolicy = _options.PropertyNamingPolicy,
            }
            : new QueryOptions();

        // Auto-resolve JsonNamingPolicy from DI if not explicitly set
        if (applyOptions.PropertyNamingPolicy is null)
        {
            var jsonOptions = context.HttpContext.RequestServices.GetService<
                IOptions<JsonOptions>
            >();
            applyOptions.PropertyNamingPolicy = jsonOptions
                ?.Value
                ?.JsonSerializerOptions
                ?.PropertyNamingPolicy;
        }

        var applyResult = queryable.Apply(query, searchBinder, applyOptions);
        if (applyResult.IsFailed)
        {
            var message = string.Join(", ", applyResult.Errors.Select(x => x.Message));
            context.Result = new BadRequestObjectResult(
                new { message, errors = applyResult.Errors }
            );
            return;
        }

        context.Result = new OkObjectResult(
            new PagedResponse<T>(applyResult.Value.Query.ToList(), applyResult.Value.Count)
        );
    }
}
