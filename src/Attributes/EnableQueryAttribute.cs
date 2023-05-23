using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public sealed class EnableQueryAttribute<T> : Attribute, IActionFilter
{
    private readonly int _topMax;

    public EnableQueryAttribute(int topMax)
    {
        _topMax = topMax;
    }

    public EnableQueryAttribute() { }

    public void OnActionExecuting(ActionExecutingContext context) { }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        var result = context.Result as ObjectResult;
        if (result is null) return;

        var content = result.Value as IQueryable<T>;
        if (content is null) return;

        var queryString = context.HttpContext.Request.Query;

        // Top
        queryString.TryGetValue("top", out var topQuery);
        var topString = topQuery.ToString();

        bool parsedTop = Int32.TryParse(topString, out int top);

        if (!string.IsNullOrEmpty(topString) && !parsedTop)
        {
            // Whatever was passed to as 'Top' was not parseable to a int
            result.StatusCode = StatusCodes.Status400BadRequest;
            result.Value = new QueryErrorResponse(StatusCodes.Status400BadRequest, "The query parameter 'Top' could not be parsed to an integer");

            return;
        }

        // Skip
        queryString.TryGetValue("skip", out var skipQuery);
        var skipString = skipQuery.ToString();

        bool parsedSkip = Int32.TryParse(skipString, out int skip);

        if (!string.IsNullOrEmpty(skipString) && !parsedSkip)
        {
            // Whatever was passed to as 'Skip' was not parseable to a int
            result.StatusCode = StatusCodes.Status400BadRequest;
            result.Value = new QueryErrorResponse(StatusCodes.Status400BadRequest, "The query parameter 'Skip' could not be parsed to an integer");

            return;
        }

        // Count
        queryString.TryGetValue("count", out var countQuery);
        var countString = countQuery.ToString();

        bool parsedCount = bool.TryParse(countString, out bool count);

        if (!string.IsNullOrEmpty(countString) && !parsedCount)
        {
            // Whatever was passed to as 'count' was not parseable to a bool
            result.StatusCode = StatusCodes.Status400BadRequest;
            result.Value = new QueryErrorResponse(StatusCodes.Status400BadRequest, "The query parameter 'Count' could not be parsed to a boolean");

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
            Top = !string.IsNullOrEmpty(topString) ? top : _topMax,
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
            var (data, totalCount) = content.Apply(query, _topMax, searchBinder);

            result.StatusCode = StatusCodes.Status200OK;
            result.Value = new PagedResponse<T>((IQueryable<T>)data, totalCount);
        }
        catch (Exception ex)
        {
            result.StatusCode = StatusCodes.Status400BadRequest;
            result.Value = new QueryErrorResponse(StatusCodes.Status400BadRequest, ex.Message);
        }
    }
}