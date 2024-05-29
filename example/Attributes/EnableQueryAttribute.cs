using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public sealed class EnableQueryAttribute<T> : ActionFilterAttribute
{
    private readonly QueryOptions? _options;

    public EnableQueryAttribute(QueryOptions options)
    {
        _options = options;
    }

    public EnableQueryAttribute() { }

    public override void OnActionExecuting(ActionExecutingContext context) { }

    public override void OnActionExecuted(ActionExecutedContext context)
    {
        var result = context.Result as ObjectResult;
        if (result is null) return;

        var queryable = result.Value as IQueryable<T>;
        if (queryable is null) return;

        var queryString = context.HttpContext.Request.Query;

        // Top
        queryString.TryGetValue("top", out var topQuery);

        if (!int.TryParse(topQuery.ToString(), out int top) && !string.IsNullOrEmpty(topQuery))
        {
            context.Result = new BadRequestObjectResult(new { Message = "The query parameter 'Top' could not be parsed to an integer" });
            return;
        }

        // Skip
        queryString.TryGetValue("skip", out var skipQuery);
        var skipString = skipQuery.ToString();

        if (!int.TryParse(skipString, out int skip) && !string.IsNullOrEmpty(skipQuery))
        {
            context.Result = new BadRequestObjectResult(new { Message = "The query parameter 'Skip' could not be parsed to an integer" });
            return;
        }

        // Count
        queryString.TryGetValue("count", out var countQuery);
        var countString = countQuery.ToString();

        if (bool.TryParse(countString, out bool count) && !string.IsNullOrEmpty(countString))
        {
            context.Result = new BadRequestObjectResult(new { Message = "The query parameter 'Count' could not be parsed to a boolean" });
            return;
        }

        // Order by
        queryString.TryGetValue("orderby", out var orderbyQuery);

        // Select
        queryString.TryGetValue("select", out var selectQuery);

        // Search
        queryString.TryGetValue("search", out var searchQuery);
        var search = searchQuery.ToString();

        // Filter
        queryString.TryGetValue("filter", out var filterQuery);

        var query = new Query()
        {
            Top = top,
            Skip = skip,
            Count = count,
            OrderBy = orderbyQuery.ToString(),
            Select = selectQuery.ToString(),
            Search = search,
            Filter = filterQuery.ToString()
        };

        ISearchBinder<T>? searchBinder = null;

        if (!string.IsNullOrEmpty(search))
        {
            searchBinder = context.HttpContext.RequestServices.GetService(typeof(ISearchBinder<T>)) as ISearchBinder<T>;
        }

        try
        {
            var (data, totalCount) = queryable.Apply(query, searchBinder, _options);

            context.Result = new OkObjectResult(new PagedResponse<T>(data, totalCount));
        }
        catch (GoatQueryException ex)
        {
            context.Result = new BadRequestObjectResult(new { ex.Message });
        }
    }
}