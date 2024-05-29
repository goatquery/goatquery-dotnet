using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

public static class OrderByEvaluator
{
    public static IQueryable<T> Evaluate<T>(IEnumerable<OrderByStatement> statements, ParameterExpression parameterExpression, IQueryable<T> queryable)
    {
        var isAlreadyOrdered = false;

        foreach (var statement in statements)
        {
            var property = Expression.Property(parameterExpression, statement.TokenLiteral());
            var lambda = Expression.Lambda(property, parameterExpression);

            if (isAlreadyOrdered)
            {
                if (statement.Direction == OrderByDirection.Ascending)
                {
                    var method = GenericMethodOf(_ => Queryable.ThenBy<int, int>(default, default)).MakeGenericMethod(parameterExpression.Type, lambda.Body.Type);

                    queryable = (IQueryable<T>)method.Invoke(null, new object[] { queryable, lambda });
                }
                else if (statement.Direction == OrderByDirection.Descending)
                {
                    var method = GenericMethodOf(_ => Queryable.ThenByDescending<int, int>(default, default)).MakeGenericMethod(parameterExpression.Type, lambda.Body.Type);

                    queryable = (IQueryable<T>)method.Invoke(null, new object[] { queryable, lambda });
                }
            }
            else
            {
                if (statement.Direction == OrderByDirection.Ascending)
                {
                    var method = GenericMethodOf(_ => Queryable.OrderBy<int, int>(default, default)).MakeGenericMethod(parameterExpression.Type, lambda.Body.Type);

                    queryable = (IQueryable<T>)method.Invoke(null, new object[] { queryable, lambda });

                    isAlreadyOrdered = true;
                }
                else if (statement.Direction == OrderByDirection.Descending)
                {
                    var method = GenericMethodOf(_ => Queryable.OrderByDescending<int, int>(default, default)).MakeGenericMethod(parameterExpression.Type, lambda.Body.Type);

                    queryable = (IQueryable<T>)method.Invoke(null, new object[] { queryable, lambda });

                    isAlreadyOrdered = true;
                }
            }
        }

        return queryable;
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