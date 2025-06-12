using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using FluentResults;

public static class FilterEvaluator
{
    public static Result<Expression> Evaluate(QueryExpression expression, ParameterExpression parameterExpression, Dictionary<string, string> propertyMapping)
    {
        switch (expression)
        {
            case InfixExpression exp:
                if (exp.Left.GetType() == typeof(Identifier))
                {
                    if (!propertyMapping.TryGetValue(exp.Left.TokenLiteral(), out var propertyName))
                    {
                        return Result.Fail($"Invalid property '{exp.Left.TokenLiteral()}' within filter");
                    }

                    var property = Expression.Property(parameterExpression, propertyName);

                    ConstantExpression value;

                    switch (exp.Right)
                    {
                        case GuidLiteral literal:
                            value = Expression.Constant(literal.Value, property.Type);
                            break;
                        case IntegerLiteral literal:
                            var integerConstant = GetIntegerExpressionConstant(literal.Value, property.Type);
                            if (integerConstant.IsFailed)
                            {
                                return Result.Fail(integerConstant.Errors);
                            }

                            value = integerConstant.Value;
                            break;
                        case DecimalLiteral literal:
                            value = Expression.Constant(literal.Value, property.Type);
                            break;
                        case FloatLiteral literal:
                            value = Expression.Constant(literal.Value, property.Type);
                            break;
                        case DoubleLiteral literal:
                            value = Expression.Constant(literal.Value, property.Type);
                            break;
                        case StringLiteral literal:
                            value = Expression.Constant(literal.Value, property.Type);
                            break;
                        case DateTimeLiteral literal:
                            value = Expression.Constant(literal.Value, property.Type);
                            break;
                        case DateLiteral literal:
                            if (property.Type == typeof(DateTime?))
                            {
                                value = Expression.Constant(literal.Value.Date, typeof(DateTime));
                            }
                            else
                            {
                                property = Expression.Property(property, "Date");
                                value = Expression.Constant(literal.Value.Date, property.Type);
                            }
                            break;
                        case NullLiteral _:
                            value = Expression.Constant(null, property.Type);
                            break;
                        default:
                            return Result.Fail($"Unsupported literal type: {exp.Right.GetType().Name}");
                    }

                    // Check if we need special nullable handling
                    var specialComparison = CreateNullableComparison(property, value, exp.Right, exp.Operator);
                    if (specialComparison != null)
                    {
                        return specialComparison;
                    }

                    switch (exp.Operator)
                    {
                        case Keywords.Eq:
                            return Expression.Equal(property, value);
                        case Keywords.Ne:
                            return Expression.NotEqual(property, value);
                        case Keywords.Contains:
                            var identifier = (Identifier)exp.Left;

                            var toLowerMethod = identifier.Value.GetType().GetMethod("ToLower", Type.EmptyTypes);

                            var propertyToLower = Expression.Call(property, toLowerMethod);
                            var valueToLower = Expression.Call(value, toLowerMethod);

                            var containsMethod = identifier.Value.GetType().GetMethod("Contains", new[] { value?.Value.GetType() });

                            return Expression.Call(propertyToLower, containsMethod, valueToLower);
                        case Keywords.Lt:
                            return Expression.LessThan(property, value);
                        case Keywords.Lte:
                            return Expression.LessThanOrEqual(property, value);
                        case Keywords.Gt:
                            return Expression.GreaterThan(property, value);
                        case Keywords.Gte:
                            return Expression.GreaterThanOrEqual(property, value);
                        default:
                            return Result.Fail($"Unsupported operator: {exp.Operator}");
                    }
                }

                var left = Evaluate(exp.Left, parameterExpression, propertyMapping);
                if (left.IsFailed)
                {
                    return left;
                }

                var right = Evaluate(exp.Right, parameterExpression, propertyMapping);
                if (right.IsFailed)
                {
                    return right;
                }

                switch (exp.Operator)
                {
                    case Keywords.And:
                        return Expression.AndAlso(left.Value, right.Value);
                    case Keywords.Or:
                        return Expression.OrElse(left.Value, right.Value);
                }

                break;
        }

        return null;
    }

    private static Expression CreateNullableComparison(MemberExpression property, ConstantExpression value, QueryExpression rightExpression, string operatorKeyword)
    {
        if (property.Type == typeof(DateTime?) && rightExpression is DateLiteral)
        {
            return CreateNullableDateComparison(property, value, operatorKeyword);
        }

        return null;
    }

    private static Expression CreateNullableDateComparison(MemberExpression property, ConstantExpression value, string operatorKeyword)
    {
        var hasValueProperty = Expression.Property(property, "HasValue");
        var valueProperty = Expression.Property(property, "Value");
        var dateProperty = Expression.Property(valueProperty, "Date");

        Expression dateComparison = operatorKeyword switch
        {
            Keywords.Eq => Expression.Equal(dateProperty, value),
            Keywords.Ne => Expression.NotEqual(dateProperty, value),
            Keywords.Lt => Expression.LessThan(dateProperty, value),
            Keywords.Lte => Expression.LessThanOrEqual(dateProperty, value),
            Keywords.Gt => Expression.GreaterThan(dateProperty, value),
            Keywords.Gte => Expression.GreaterThanOrEqual(dateProperty, value),
            _ => throw new ArgumentException($"Unsupported operator for nullable date comparison: {operatorKeyword}")
        };

        return operatorKeyword == Keywords.Ne
            ? Expression.OrElse(Expression.Not(hasValueProperty), dateComparison)
            : Expression.AndAlso(hasValueProperty, dateComparison);
    }

    private static Result<ConstantExpression> GetIntegerExpressionConstant(int value, Type targetType)
    {
        try
        {
            // Fetch the underlying type if it's nullable.
            var underlyingType = Nullable.GetUnderlyingType(targetType);
            var type = underlyingType ?? targetType;

            object convertedValue = type switch
            {
                Type t when t == typeof(int) => value,
                Type t when t == typeof(long) => Convert.ToInt64(value),
                Type t when t == typeof(short) => Convert.ToInt16(value),
                Type t when t == typeof(byte) => Convert.ToByte(value),
                Type t when t == typeof(uint) => Convert.ToUInt32(value),
                Type t when t == typeof(ulong) => Convert.ToUInt64(value),
                Type t when t == typeof(ushort) => Convert.ToUInt16(value),
                Type t when t == typeof(sbyte) => Convert.ToSByte(value),
                _ => throw new NotSupportedException($"Unsupported numeric type: {targetType.Name}")
            };

            return Expression.Constant(convertedValue, targetType);
        }
        catch (OverflowException)
        {
            return Result.Fail($"Value {value} is too large for type {targetType.Name}");
        }
        catch (Exception ex)
        {
            return Result.Fail($"Error converting {value} to {targetType.Name}: {ex.Message}");
        }
    }
}