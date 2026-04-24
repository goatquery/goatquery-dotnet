using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using FluentResults;

public static class OrderByEvaluator
{
    public static Result<IQueryable<T>> Evaluate<T>(
        IEnumerable<OrderByStatement> statements,
        ParameterExpression parameterExpression,
        IQueryable<T> queryable,
        PropertyMappingTree propertyMappingTree
    )
    {
        var isAlreadyOrdered = false;

        foreach (var statement in statements)
        {
            var propertyResult = BuildPropertyExpression(
                statement,
                parameterExpression,
                propertyMappingTree
            );
            if (propertyResult.IsFailed)
                return Result.Fail(propertyResult.Errors);

            var property = propertyResult.Value;
            var lambda = Expression.Lambda(property, parameterExpression);

            var methodName = GetOrderByMethodName(statement.Direction, isAlreadyOrdered);
            var method = GetQueryableMethod(methodName)
                .MakeGenericMethod(parameterExpression.Type, lambda.Body.Type);
            queryable = (IQueryable<T>)method.Invoke(null, new object[] { queryable, lambda });

            isAlreadyOrdered = true;
        }

        return Result.Ok(queryable);
    }

    private static Result<Expression> BuildPropertyExpression(
        OrderByStatement statement,
        ParameterExpression parameterExpression,
        PropertyMappingTree propertyMappingTree
    )
    {
        Expression current = parameterExpression;
        var currentMappingTree = propertyMappingTree;

        foreach (
            var (segment, isLast) in statement.Segments.Select(
                (s, i) => (s, i == statement.Segments.Count - 1)
            )
        )
        {
            if (!currentMappingTree.TryGetProperty(segment, out var propertyNode))
            {
                return Result.Fail($"Invalid property '{segment}' within orderby");
            }

            current = Expression.Property(current, propertyNode.ActualPropertyName);

            if (!isLast)
            {
                if (!propertyNode.HasNestedMapping)
                    return Result.Fail(
                        $"Property '{segment}' does not support nested navigation in orderby"
                    );

                currentMappingTree = propertyNode.NestedMapping;
            }
        }

        return Result.Ok(current);
    }

    private static string GetOrderByMethodName(OrderByDirection direction, bool isAlreadyOrdered)
    {
        return (direction, isAlreadyOrdered) switch
        {
            (OrderByDirection.Ascending, false) => nameof(Queryable.OrderBy),
            (OrderByDirection.Descending, false) => nameof(Queryable.OrderByDescending),
            (OrderByDirection.Ascending, true) => nameof(Queryable.ThenBy),
            (OrderByDirection.Descending, true) => nameof(Queryable.ThenByDescending),
            _ => nameof(Queryable.OrderBy),
        };
    }

    private static MethodInfo GetQueryableMethod(string methodName)
    {
        return typeof(Queryable)
            .GetMethods()
            .First(m => m.Name == methodName && m.GetParameters().Length == 2);
    }
}
