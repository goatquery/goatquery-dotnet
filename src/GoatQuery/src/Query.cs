/// <summary>
/// Represents the query parameters for filtering, ordering, pagination, and search.
/// Query parameter names do not use the OData <c>$</c> prefix.
/// </summary>
public sealed class Query
{
    /// <summary>Maximum number of items to return.</summary>
    public int? Top { get; set; }

    /// <summary>Number of items to skip before returning results.</summary>
    public int? Skip { get; set; }

    /// <summary>When <c>true</c>, includes the total count of matching items in the result.</summary>
    public bool? Count { get; set; }

    /// <summary>
    /// Comma-separated list of property names with optional direction (<c>asc</c>/<c>desc</c>).
    /// Nested properties use forward-slash syntax (e.g. <c>company/name asc</c>).
    /// </summary>
    public string OrderBy { get; set; } = string.Empty;

    /// <summary>
    /// Free-text search term. Requires an <see cref="ISearchBinder{T}"/> implementation to take effect.
    /// </summary>
    public string Search { get; set; } = string.Empty;

    /// <summary>
    /// Filter expression using infix operators (e.g. <c>age gt 18 and name eq 'John'</c>).
    /// Supports comparison (<c>eq</c>, <c>ne</c>, <c>gt</c>, <c>gte</c>, <c>lt</c>, <c>lte</c>),
    /// string (<c>contains</c>), logical (<c>and</c>, <c>or</c>), grouped expressions, and
    /// lambda expressions (<c>any</c>/<c>all</c>) for collection filtering.
    /// </summary>
    public string Filter { get; set; } = string.Empty;
}
