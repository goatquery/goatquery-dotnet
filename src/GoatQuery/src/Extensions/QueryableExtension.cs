using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json.Serialization;

public static class QueryableExtension
{
    private static Dictionary<string, string> CreatePropertyMapping<T>()
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var properties = typeof(T).GetProperties();

        foreach (var property in properties)
        {
            var jsonPropertyNameAttribute = property.GetCustomAttribute<JsonPropertyNameAttribute>();
            if (jsonPropertyNameAttribute != null)
            {
                result[jsonPropertyNameAttribute.Name] = property.Name;
                continue;
            }

            result[property.Name] = property.Name;
        }

        return result;
    }

    public static (IQueryable<T>, int?) Apply<T>(this IQueryable<T> queryable, Query query, ISearchBinder<T> searchBinder = null, QueryOptions options = null)
    {
        if (query.Top > options?.MaxTop)
        {
            throw new GoatQueryException("The value supplied for the query parameter 'Top' was greater than the maximum top allowed for this resource");
        }

        var type = typeof(T);

        var propertyMappings = CreatePropertyMapping<T>();

        // Filter
        if (!string.IsNullOrEmpty(query.Filter))
        {
            var lexer = new QueryLexer(query.Filter);
            var parser = new QueryParser(lexer);
            var statement = parser.ParseFilter();

            ParameterExpression parameter = Expression.Parameter(type);

            var expression = FilterEvaluator.Evaluate(statement.Expression, parameter, propertyMappings);

            var exp = Expression.Lambda<Func<T, bool>>(expression, parameter);

            queryable = queryable.Where(exp);
        }

        // Search
        if (searchBinder != null && !string.IsNullOrEmpty(query.Search))
        {
            var searchExpression = searchBinder.Bind(query.Search);

            if (searchExpression is null)
            {
                throw new GoatQueryException("Cannot parse search binder expression");
            }

            queryable = queryable.Where(searchExpression);
        }

        // Count
        int? count = null;

        if (query.Count ?? false)
        {
            count = queryable.Count();
        }

        // Order by
        if (!string.IsNullOrEmpty(query.OrderBy))
        {
            var lexer = new QueryLexer(query.OrderBy);
            var parser = new QueryParser(lexer);

            var statements = parser.ParseOrderBy();

            var parameter = Expression.Parameter(type);

            queryable = OrderByEvaluator.Evaluate<T>(statements, parameter, queryable, propertyMappings);
        }

        // Skip
        if (query.Skip > 0)
        {
            queryable = queryable.Skip(query.Skip ?? 0);
        }

        // Top
        if (query.Top > 0)
        {
            queryable = queryable.Take(query.Top ?? 0);
        }

        if (query.Top <= 0 && options?.MaxTop != null)
        {
            queryable = queryable.Take(options.MaxTop);
        }

        return (queryable, count);
    }
}