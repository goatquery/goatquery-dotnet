namespace GoatQuery;

using System;
using System.Linq;
using System.Linq.Expressions;
using FluentResults;

/// <summary>
/// Extension methods for applying <see cref="Query"/> to <see cref="IQueryable{T}"/> sources.
/// </summary>
public static class QueryableExtension
{
    /// <summary>
    /// Applies filtering, search, ordering, pagination, and optional count to the queryable source.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="queryable">The source queryable.</param>
    /// <param name="query">The query parameters to apply.</param>
    /// <param name="searchBinder">
    /// Optional search binder that defines how the <c>search</c> parameter is translated to a predicate.
    /// </param>
    /// <param name="options">Optional configuration for max top, property depth, and naming policy.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing the <see cref="QueryResult{T}"/> on success,
    /// or error details on failure (e.g. invalid filter syntax, unknown property, exceeded max top).
    /// </returns>
    public static Result<QueryResult<T>> Apply<T>(
        this IQueryable<T> queryable,
        Query query,
        ISearchBinder<T> searchBinder = null,
        QueryOptions options = null
    )
    {
        if (query.Top > options?.MaxTop)
        {
            return Result.Fail(
                "The value supplied for the query parameter 'Top' was greater than the maximum top allowed for this resource"
            );
        }

        var type = typeof(T);

        var maxDepth = options?.MaxPropertyMappingDepth ?? 5;
        var namingPolicy = options?.PropertyNamingPolicy;
        var propertyMappingTree = PropertyMappingTreeBuilder.BuildMappingTree<T>(
            maxDepth,
            namingPolicy
        );

        // Filter
        if (!string.IsNullOrEmpty(query.Filter))
        {
            var lexer = new QueryLexer(query.Filter);
            var parser = new QueryParser(lexer);
            var statement = parser.ParseFilter();
            if (statement.IsFailed)
            {
                return Result.Fail(statement.Errors);
            }

            ParameterExpression parameter = Expression.Parameter(type);

            var expression = FilterEvaluator.Evaluate(
                statement.Value,
                parameter,
                propertyMappingTree,
                maxDepth
            );
            if (expression.IsFailed)
            {
                return Result.Fail(expression.Errors);
            }

            var exp = Expression.Lambda<Func<T, bool>>(expression.Value, parameter);

            queryable = queryable.Where(exp);
        }

        // Search
        if (searchBinder != null && !string.IsNullOrEmpty(query.Search))
        {
            var searchExpression = searchBinder.Bind(query.Search);

            if (searchExpression is null)
            {
                return Result.Fail("Cannot parse search binder expression.");
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

            var orderByQuery = OrderByEvaluator.Evaluate<T>(
                statements,
                parameter,
                queryable,
                propertyMappingTree
            );
            if (orderByQuery.IsFailed)
            {
                return Result.Fail(orderByQuery.Errors);
            }

            queryable = orderByQuery.Value;
        }

        // Skip
        if (query.Skip > 0)
        {
            queryable = queryable.Skip(query.Skip.Value);
        }

        // Top
        if (query.Top > 0)
        {
            queryable = queryable.Take(query.Top.Value);
        }

        if ((query.Top == null || query.Top <= 0) && options?.MaxTop > 0)
        {
            queryable = queryable.Take(options.MaxTop);
        }

        return Result.Ok(new QueryResult<T>(queryable, count));
    }
}
