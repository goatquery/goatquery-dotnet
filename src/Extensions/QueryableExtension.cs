using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

public static class QueryableExtension
{
    public static (IQueryable<T>, int?) Apply<T>(this IQueryable<T> queryable, Query query, ISearchBinder<T>? searchBinder = null, QueryOptions? options = null)
    {
        if (query.Top > options?.MaxTop)
        {
            throw new GoatQueryException("The value supplied for the query parameter 'Top' was greater than the maximum top allowed for this resource");
        }

        var type = typeof(T);

        // Filter
        if (!string.IsNullOrEmpty(query.Filter))
        {
            var lexer = new QueryLexer(query.Filter);
            var parser = new QueryParser(lexer);
            var statements = parser.ParseFilter();

            ParameterExpression parameter = Expression.Parameter(type);
            Expression? binaryExpression = null;

            foreach (var statement in statements)
            {
                var property = Expression.Property(parameter, statement.Left.TokenLiteral());
                ConstantExpression? value = null;

                switch (statement.Right)
                {
                    case IntegerLiteral literal:
                        value = Expression.Constant(literal.Value, property.Type);
                        break;
                    case StringLiteral literal:
                        value = Expression.Constant(literal.Value, property.Type);
                        break;
                    default:
                        break;
                }

                var expression = Expression.Equal(property, value);

                binaryExpression = binaryExpression == null ? expression : Expression.AndAlso(binaryExpression, expression);
            }

            var exp = Expression.Lambda<Func<T, bool>>(binaryExpression, parameter);

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
            var isAlreadyOrdered = false;

            var parameter = Expression.Parameter(type);

            foreach (var statement in statements)
            {
                var property = Expression.Property(parameter, statement.TokenLiteral());
                var lamba = Expression.Lambda(property, parameter);

                if (isAlreadyOrdered)
                {
                    if (statement.Direction == OrderByDirection.Ascending)
                    {
                        var method = GenericMethodOf(_ => Queryable.ThenBy<int, int>(default, default)).MakeGenericMethod(type, lamba.Body.Type);

                        queryable = (IQueryable<T>)method.Invoke(null, new object[] { queryable, lamba });
                    }
                    else if (statement.Direction == OrderByDirection.Descending)
                    {
                        var method = GenericMethodOf(_ => Queryable.ThenByDescending<int, int>(default, default)).MakeGenericMethod(type, lamba.Body.Type);

                        queryable = (IQueryable<T>)method.Invoke(null, new object[] { queryable, lamba });
                    }
                }
                else
                {
                    if (statement.Direction == OrderByDirection.Ascending)
                    {
                        var method = GenericMethodOf(_ => Queryable.OrderBy<int, int>(default, default)).MakeGenericMethod(type, lamba.Body.Type);

                        queryable = (IQueryable<T>)method.Invoke(null, new object[] { queryable, lamba });

                        isAlreadyOrdered = true;
                    }
                    else if (statement.Direction == OrderByDirection.Descending)
                    {
                        var method = GenericMethodOf(_ => Queryable.OrderByDescending<int, int>(default, default)).MakeGenericMethod(type, lamba.Body.Type);

                        queryable = (IQueryable<T>)method.Invoke(null, new object[] { queryable, lamba });

                        isAlreadyOrdered = true;
                    }
                }
            }
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

        Console.WriteLine(queryable.ToString());

        return (queryable, count);
    }

    private static MethodInfo GenericMethodOf<TReturn>(Expression<Func<object, TReturn>> expression)
    {
        return GenericMethodOf(expression as Expression);
    }

    private static MethodInfo GenericMethodOf(Expression expression)
    {
        LambdaExpression lambdaExpression = (LambdaExpression)expression;

        return ((MethodCallExpression)lambdaExpression.Body).Method.GetGenericMethodDefinition();
    }
}