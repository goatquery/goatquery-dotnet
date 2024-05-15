using System.Collections.Generic;
using System.Linq;

public static class QueryableExtension
{
    public static Dictionary<string, string> _filterOperations => new Dictionary<string, string>
    {
        {"eq", "=="},
        {"ne", "!="},
        {"contains", "Contains"},
    };

    public static (IQueryable<T>, int?) Apply<T>(this IQueryable<T> queryable, Query query)
    {
        // Order by
        if (!string.IsNullOrEmpty(query.OrderBy))
        {
            var lexer = new QueryLexer(query.OrderBy);
            var parser = new QueryParser(lexer);

            parser.ParseOrderBy();

            // queryable = queryable.OrderBy();
        }

        return (queryable, 0);
    }
}