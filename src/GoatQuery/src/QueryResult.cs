namespace GoatQuery;

using System.Linq;

/// <summary>
/// Contains the result of applying a <see cref="Query"/> to an <see cref="IQueryable{T}"/>.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public sealed class QueryResult<T>
{
    public QueryResult(IQueryable<T> query, int? count)
    {
        Query = query;
        Count = count;
    }

    /// <summary>The queryable with filters, ordering, and pagination applied.</summary>
    public IQueryable<T> Query { get; }

    /// <summary>
    /// Total count of matching items before pagination, or <c>null</c> if
    /// <see cref="GoatQuery.Query.Count"/> was not set to <c>true</c>.
    /// </summary>
    public int? Count { get; }
}
