using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

public static class QueryableExtension
{
    public static (IQueryable<T>, int?) Apply<T>(this IQueryable<T> queryable, Query query)
    {
        var type = typeof(T);

        // Order by
        if (!string.IsNullOrEmpty(query.OrderBy))
        {
            var lexer = new QueryLexer(query.OrderBy);
            var parser = new QueryParser(lexer);

            var statements = parser.ParseOrderBy();
            var isAlreadyOrdered = false;

            foreach (var statement in statements)
            {
                ParameterExpression parameter = Expression.Parameter(type);
                MemberExpression property = Expression.Property(parameter, statement.TokenLiteral());
                LambdaExpression lamba = Expression.Lambda(property, parameter);

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

        return (queryable, 0);
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