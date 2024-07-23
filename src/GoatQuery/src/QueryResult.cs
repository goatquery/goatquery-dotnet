using System.Linq;

public sealed class QueryResult<T>
{
    public QueryResult(IQueryable<T> query, int? count)
    {
        Query = query;
        Count = count;
    }

    public IQueryable<T> Query { get; set; }
    public int? Count { get; set; }
}