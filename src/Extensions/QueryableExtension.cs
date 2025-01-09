using System.Linq.Dynamic.Core;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;

public static class QueryableExtension
{
    public static Dictionary<string, string> _filterOperations => new Dictionary<string, string>
    {
        {"eq", "=="},
        {"ne", "!="},
        {"contains", "Contains"},
    };

    public static (IQueryable, int?) Apply<T>(this IQueryable<T> queryable, Query query, int? maxTop = null, ISearchBinder<T>? searchBinder = null)
    {
        var result = (IQueryable)queryable;

        if (maxTop is not null && query.Top > maxTop)
        {
            throw new GoatQueryException("The value supplied for the query parameter 'Top' was greater than the maximum top allowed for this resource");
        }

        // Filter
        if (!string.IsNullOrEmpty(query.Filter))
        {
            var filters = StringHelper.SplitString(query.Filter);

            var where = new StringBuilder();

            for (int i = 0; i < filters.Count; i++)
            {
                var filter = filters[i];
                var opts = StringHelper.SplitStringByWhitespace(filter.Trim());

                if (opts.Count != 3)
                {
                    continue;
                }

                if (i > 0)
                {
                    var prev = filters[i - 1];
                    where.Append($" {prev.Trim()} ");
                }

                var property = opts[0].Replace("\'", string.Empty);
                var operand = opts[1];
                var value = opts[2].Replace("'", "\"");

                string? propertyName = typeof(T).GetProperties().FirstOrDefault(x => x.Name == property)?.Name;

                if (!string.IsNullOrEmpty(propertyName))
                {
                    property = propertyName;
                }

                if (operand.Equals("contains", StringComparison.OrdinalIgnoreCase))
                {
                    where.Append($"{property}.{_filterOperations[operand]}({value})");
                }
                else if (typeof(T).GetProperties().FirstOrDefault(x => x.Name.Equals(property, StringComparison.OrdinalIgnoreCase))?.PropertyType == typeof(string))
                {
                    where.Append($"{property}.ToLower() {_filterOperations[operand]} {value}.ToLower()");
                }
                else if (typeof(T).GetProperties().FirstOrDefault(x => x.Name.Equals(property, StringComparison.OrdinalIgnoreCase))?.PropertyType == typeof(Guid))
                {
                    where.Append($"{property} {_filterOperations[operand]} Guid({value})");
                }
                else
                {
                    where.Append($"{property} {_filterOperations[operand]} {value}");
                }
            }

            result = result.Where(where.ToString());
        }

        // Search
        if (searchBinder is not null && !string.IsNullOrEmpty(query.Search))
        {
            var searchExpression = searchBinder.Bind(query.Search);

            if (searchExpression is null)
            {
                throw new GoatQueryException("search binder does not return valid expression that can be parsed to where clause");
            }

            result = result.Where(searchExpression);
        }

        int? count = null;

        // Count
        if (query.Count ?? false)
        {
            count = result.Count();
        }

        // Order by
        if (!string.IsNullOrEmpty(query.OrderBy))
        {
            result = result.OrderBy(query.OrderBy);
        }

        // Select
        if (!string.IsNullOrEmpty(query.Select))
        {
            result = result.Select($"new {{ {query.Select} }}");
        }

        // Skip
        if (query.Skip > 0)
        {
            result = result.Skip(query.Skip ?? 0);
        }

        // Top
        if (query.Top > 0)
        {
            result = result.Take(query.Top ?? 0);
        }

        return (result, count);
    }
}
