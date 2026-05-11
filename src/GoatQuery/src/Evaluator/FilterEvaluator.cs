namespace GoatQuery;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json.Serialization;
using FluentResults;

internal static class FilterEvaluator
{
    private const string HasValuePropertyName = "HasValue";
    private const string ValuePropertyName = "Value";

    private static readonly MethodInfo EnumerableAnyWithPredicate = GetEnumerableMethod("Any", 2);
    private static readonly MethodInfo EnumerableAnyWithoutPredicate = GetEnumerableMethod(
        "Any",
        1
    );
    private static readonly MethodInfo EnumerableAllWithPredicate = GetEnumerableMethod("All", 2);
    private static readonly MethodInfo StringToLowerMethod = GetStringMethod("ToLower");
    private static readonly MethodInfo StringContainsMethod = GetStringMethod(
        "Contains",
        typeof(string)
    );

    private static MethodInfo GetEnumerableMethod(string methodName, int parameterCount) =>
        typeof(Enumerable)
            .GetMethods()
            .First(m => m.Name == methodName && m.GetParameters().Length == parameterCount);

    private static MethodInfo GetStringMethod(string methodName, params Type[] parameterTypes) =>
        typeof(string).GetMethod(methodName, parameterTypes ?? Type.EmptyTypes);

    public static Result<Expression> Evaluate(
        QueryExpression expression,
        ParameterExpression parameterExpression,
        PropertyMappingTree propertyMappingTree,
        int maxPropertyMappingDepth = 5
    )
    {
        if (expression == null)
            return Result.Fail("Expression cannot be null.");
        if (parameterExpression == null)
            return Result.Fail("Parameter expression cannot be null.");
        if (propertyMappingTree == null)
            return Result.Fail("Property mapping tree cannot be null.");

        var context = new FilterEvaluationContext(
            parameterExpression,
            propertyMappingTree,
            maxPropertyMappingDepth
        );
        return EvaluateExpression(expression, context);
    }

    private static Result<Expression> EvaluateExpression(
        QueryExpression expression,
        FilterEvaluationContext context
    )
    {
        return expression switch
        {
            InfixExpression exp => EvaluateInfixExpression(exp, context),
            QueryLambdaExpression lambdaExp => EvaluateLambdaExpression(lambdaExp, context),
            _ => Result.Fail($"Unsupported expression type '{expression.GetType().Name}'."),
        };
    }

    private static Result<Expression> EvaluatePropertyPathExpression(
        InfixExpression exp,
        PropertyPath propertyPath,
        FilterEvaluationContext context
    )
    {
        var baseExpression = context.GetBaseExpression();

        var propertyPathResult = BuildPropertyPath(
            propertyPath,
            baseExpression,
            context.PropertyMappingTree
        );
        if (propertyPathResult.IsFailed)
            return Result.Fail(propertyPathResult.Errors);

        var finalProperty = propertyPathResult.Value;

        if (exp.Right is NullLiteral)
        {
            return CreateNullComparison(exp, finalProperty);
        }

        return EvaluateValueComparison(exp, finalProperty);
    }

    private static Result<MemberExpression> BuildPropertyPath(
        PropertyPath propertyPath,
        Expression startExpression,
        PropertyMappingTree propertyMappingTree
    )
    {
        var result = propertyMappingTree.WalkPropertyPath(propertyPath.Segments, startExpression);
        if (result.IsFailed)
            return Result.Fail(result.Errors);
        return Result.Ok((MemberExpression)result.Value);
    }

    private static Result<MemberExpression> ResolvePropertyPathForCollection(
        PropertyPath propertyPath,
        Expression baseExpression,
        PropertyMappingTree propertyMappingTree
    )
    {
        var result = propertyMappingTree.WalkPropertyPath(propertyPath.Segments, baseExpression);
        if (result.IsFailed)
            return Result.Fail(result.Errors);
        return Result.Ok((MemberExpression)result.Value);
    }

    private static Result<Expression> CreateNullComparison(
        InfixExpression exp,
        MemberExpression property
    )
    {
        var underlyingType = Nullable.GetUnderlyingType(property.Type);
        if (underlyingType == null && property.Type.IsValueType)
        {
            // Non-nullable value types can never be null:
            // eq null → always false (no matches), ne null → always true (all match)
            return exp.Operator == Keywords.Eq
                ? Expression.Constant(false)
                : Expression.Constant(true);
        }

        return exp.Operator == Keywords.Eq
            ? Expression.Equal(property, Expression.Constant(null, property.Type))
            : Expression.NotEqual(property, Expression.Constant(null, property.Type));
    }

    private static bool IsDateLiteralComparison(
        Expression property,
        QueryExpression rightExpression
    )
    {
        if (!(rightExpression is DateLiteral))
            return false;

        var underlyingType = Nullable.GetUnderlyingType(property.Type) ?? property.Type;
        return underlyingType == typeof(DateTime) || underlyingType == typeof(DateTimeOffset);
    }

    private static Result<Expression> CreateDateRangeExpression(
        Expression property,
        DateLiteral dateLiteral,
        string operatorKeyword
    )
    {
        var underlyingType = Nullable.GetUnderlyingType(property.Type) ?? property.Type;
        var isNullable = Nullable.GetUnderlyingType(property.Type) != null;

        ConstantExpression startOfDay,
            startOfNextDay;

        if (underlyingType == typeof(DateTimeOffset))
        {
            startOfDay = Expression.Constant(
                new DateTimeOffset(dateLiteral.Value.Date, TimeSpan.Zero),
                underlyingType
            );
            startOfNextDay = Expression.Constant(
                new DateTimeOffset(dateLiteral.Value.Date.AddDays(1), TimeSpan.Zero),
                underlyingType
            );
        }
        else
        {
            startOfDay = Expression.Constant(dateLiteral.Value.Date, underlyingType);
            startOfNextDay = Expression.Constant(dateLiteral.Value.Date.AddDays(1), underlyingType);
        }

        Expression prop = isNullable ? Expression.Property(property, ValuePropertyName) : property;

        Expression comparison = operatorKeyword switch
        {
            Keywords.Eq => Expression.AndAlso(
                Expression.GreaterThanOrEqual(prop, startOfDay),
                Expression.LessThan(prop, startOfNextDay)
            ),
            Keywords.Ne => Expression.OrElse(
                Expression.LessThan(prop, startOfDay),
                Expression.GreaterThanOrEqual(prop, startOfNextDay)
            ),
            Keywords.Lt => Expression.LessThan(prop, startOfDay),
            Keywords.Lte => Expression.LessThan(prop, startOfNextDay),
            Keywords.Gt => Expression.GreaterThanOrEqual(prop, startOfNextDay),
            Keywords.Gte => Expression.GreaterThanOrEqual(prop, startOfDay),
            _ => throw new InvalidOperationException(
                $"Unsupported operator '{operatorKeyword}' for date comparison."
            ),
        };

        if (isNullable)
        {
            var hasValue = Expression.Property(property, HasValuePropertyName);
            comparison =
                operatorKeyword == Keywords.Ne
                    ? Expression.OrElse(Expression.Not(hasValue), comparison)
                    : Expression.AndAlso(hasValue, comparison);
        }

        return comparison;
    }

    private static Result<Expression> EvaluateValueComparison(
        InfixExpression exp,
        Expression expression
    )
    {
        if (IsDateLiteralComparison(expression, exp.Right))
        {
            return CreateDateRangeExpression(expression, (DateLiteral)exp.Right, exp.Operator);
        }

        var valueResult = CreateConstantExpression(exp.Right, expression);
        if (valueResult.IsFailed)
        {
            if (exp.Right is DoubleLiteral || exp.Right is IntegerLiteral)
            {
                var promotionResult = TryCreateNumericPromotion(
                    exp.Operator,
                    expression,
                    exp.Right
                );
                if (promotionResult.IsSuccess)
                    return promotionResult;
            }

            return Result.Fail(valueResult.Errors);
        }

        return CreateComparisonExpression(exp.Operator, expression, valueResult.Value);
    }

    /// <summary>
    /// When a numeric literal can't be exactly represented in the target property type
    /// (e.g., 1.5 on an int property), this method promotes both sides to double.
    /// For eq/ne with a fractional value, the result is a constant (no integer equals 1.5).
    /// For ordering operators, both sides are cast to double for a mathematically correct comparison.
    /// If the double value is a whole number (e.g., 5.0), it converts to the target integer type instead.
    /// </summary>
    private static Result<Expression> TryCreateNumericPromotion(
        string operatorKeyword,
        Expression property,
        QueryExpression literal
    )
    {
        var propertyType = GetNonNullableType(property.Type);
        if (!IsIntegerType(propertyType))
            return Result.Fail("Cannot promote value to a numeric type.");

        var raw = literal.TokenLiteral();
        if (
            !double.TryParse(
                raw,
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture,
                out var doubleValue
            )
        )
            return Result.Fail($"Cannot parse '{raw}' as a valid number.");

        // If the double value is a whole number (e.g., 5.0), try to convert it to the
        // target integer type and do an exact comparison instead of promoting to double.
        if (doubleValue == Math.Truncate(doubleValue))
        {
            try
            {
                var longValue = Convert.ToInt64(doubleValue);
                var intResult = GetIntegerExpressionConstant(longValue, property.Type);
                if (intResult.IsSuccess)
                {
                    return CreateComparisonExpression(operatorKeyword, property, intResult.Value);
                }
            }
            catch (OverflowException)
            {
                // Fall through to double promotion
            }
        }

        // Fractional value: no integer can exactly equal it
        if (operatorKeyword == Keywords.Eq)
            return Result.Ok((Expression)Expression.Constant(false));

        if (operatorKeyword == Keywords.Ne)
            return Result.Ok((Expression)Expression.Constant(true));

        // For ordering: promote the property to double and compare
        var convertedProperty = Expression.Convert(property, typeof(double));
        var doubleConstant = Expression.Constant(doubleValue);

        return operatorKeyword switch
        {
            Keywords.Lt => Result.Ok(
                (Expression)Expression.LessThan(convertedProperty, doubleConstant)
            ),
            Keywords.Lte => Result.Ok(
                (Expression)Expression.LessThanOrEqual(convertedProperty, doubleConstant)
            ),
            Keywords.Gt => Result.Ok(
                (Expression)Expression.GreaterThan(convertedProperty, doubleConstant)
            ),
            Keywords.Gte => Result.Ok(
                (Expression)Expression.GreaterThanOrEqual(convertedProperty, doubleConstant)
            ),
            _ => Result.Fail($"Unsupported operator '{operatorKeyword}' for numeric promotion."),
        };
    }

    private static bool IsIntegerType(Type type)
    {
        return type == typeof(int)
            || type == typeof(long)
            || type == typeof(short)
            || type == typeof(byte)
            || type == typeof(uint)
            || type == typeof(ulong)
            || type == typeof(ushort)
            || type == typeof(sbyte);
    }

    private static Result<Expression> CreateComparisonExpression(
        string operatorKeyword,
        Expression expression,
        ConstantExpression value
    )
    {
        return operatorKeyword switch
        {
            Keywords.Eq => CreateEqualityExpression(expression, value, isEqual: true),
            Keywords.Ne => CreateEqualityExpression(expression, value, isEqual: false),
            Keywords.Contains => CreateContainsExpression(expression, value),
            Keywords.Lt => CreateOrderingExpression(Expression.LessThan, expression, value, "lt"),
            Keywords.Lte => CreateOrderingExpression(
                Expression.LessThanOrEqual,
                expression,
                value,
                "lte"
            ),
            Keywords.Gt => CreateOrderingExpression(
                Expression.GreaterThan,
                expression,
                value,
                "gt"
            ),
            Keywords.Gte => CreateOrderingExpression(
                Expression.GreaterThanOrEqual,
                expression,
                value,
                "gte"
            ),
            _ => Result.Fail($"Unsupported operator '{operatorKeyword}'."),
        };
    }

    private static Result<Expression> CreateOrderingExpression(
        Func<Expression, Expression, BinaryExpression> factory,
        Expression left,
        Expression right,
        string operatorName
    )
    {
        var type = Nullable.GetUnderlyingType(left.Type) ?? left.Type;
        if (type == typeof(string) || type == typeof(Guid) || type == typeof(bool))
        {
            return Result.Fail(
                $"Operator '{operatorName}' is not supported for type '{type.Name}'."
            );
        }

        return Result.Ok((Expression)factory(left, right));
    }

    private static Result<ConstantExpression> CreateConstantExpression(
        QueryExpression literal,
        Expression expression
    )
    {
        return literal switch
        {
            IntegerLiteral intLit => CreateIntegerOrEnumConstant(intLit.Value, expression.Type),
            DateLiteral dateLit => Result.Ok(CreateDateConstant(dateLit, expression.Type)),
            GuidLiteral guidLit => CreateTypedConstant(guidLit.Value, expression.Type),
            DoubleLiteral dblLit => PromoteNumericLiteral(dblLit, expression.Type),
            StringLiteral strLit => CreateStringOrEnumConstant(strLit.Value, expression.Type),
            DateTimeLiteral dtLit => Result.Ok(CreateDateTimeConstant(dtLit, expression.Type)),
            BooleanLiteral boolLit => CreateTypedConstant(boolLit.Value, expression.Type),
            NullLiteral _ => Result.Fail(
                "Unexpected null literal. Null comparisons should be handled earlier."
            ),
            _ => Result.Fail($"Unsupported literal type '{literal.GetType().Name}'."),
        };
    }

    private static ConstantExpression CreateDateConstant(DateLiteral dateLiteral, Type targetType)
    {
        var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (underlyingType == typeof(DateTimeOffset))
        {
            var value = new DateTimeOffset(dateLiteral.Value.Date, TimeSpan.Zero);
            return Expression.Constant(value, targetType);
        }

        return Expression.Constant(dateLiteral.Value.Date, underlyingType);
    }

    private static Result<ConstantExpression> CreateTypedConstant<T>(T value, Type targetType)
    {
        var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (underlyingType != typeof(T))
        {
            return Result.Fail(
                $"Cannot use {typeof(T).Name} literal for property of type '{underlyingType.Name}'."
            );
        }

        return Result.Ok(Expression.Constant(value, targetType));
    }

    private static Result<ConstantExpression> PromoteNumericLiteral(
        QueryExpression literal,
        Type targetType
    )
    {
        var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;
        var raw = literal.TokenLiteral();

        try
        {
            object value = underlyingType switch
            {
                Type t when t == typeof(int) => int.Parse(raw, CultureInfo.InvariantCulture),
                Type t when t == typeof(long) => long.Parse(raw, CultureInfo.InvariantCulture),
                Type t when t == typeof(short) => short.Parse(raw, CultureInfo.InvariantCulture),
                Type t when t == typeof(byte) => byte.Parse(raw, CultureInfo.InvariantCulture),
                Type t when t == typeof(decimal) => decimal.Parse(
                    raw,
                    CultureInfo.InvariantCulture
                ),
                Type t when t == typeof(float) => float.Parse(raw, CultureInfo.InvariantCulture),
                Type t when t == typeof(double) => double.Parse(raw, CultureInfo.InvariantCulture),
                Type t when t == typeof(uint) => uint.Parse(raw, CultureInfo.InvariantCulture),
                Type t when t == typeof(ulong) => ulong.Parse(raw, CultureInfo.InvariantCulture),
                Type t when t == typeof(ushort) => ushort.Parse(raw, CultureInfo.InvariantCulture),
                Type t when t == typeof(sbyte) => sbyte.Parse(raw, CultureInfo.InvariantCulture),
                _ => throw new NotSupportedException(),
            };

            return Result.Ok(Expression.Constant(value, targetType));
        }
        catch (OverflowException)
        {
            return Result.Fail($"Value '{raw}' overflows type '{underlyingType.Name}'.");
        }
        catch (FormatException)
        {
            return Result.Fail($"Cannot convert '{raw}' to type '{underlyingType.Name}'.");
        }
        catch (NotSupportedException)
        {
            return Result.Fail(
                $"Cannot use numeric literal for property of type '{underlyingType.Name}'."
            );
        }
    }

    private static ConstantExpression CreateDateTimeConstant(
        DateTimeLiteral dtLiteral,
        Type targetType
    )
    {
        var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (underlyingType == typeof(DateTimeOffset))
        {
            if (
                DateTimeOffset.TryParse(
                    dtLiteral.TokenLiteral(),
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None,
                    out var dto
                )
            )
            {
                return Expression.Constant(dto, targetType);
            }
            return Expression.Constant(new DateTimeOffset(dtLiteral.Value), targetType);
        }

        return Expression.Constant(dtLiteral.Value, targetType);
    }

    private static Result<Expression> CreateContainsExpression(
        Expression expression,
        ConstantExpression value
    )
    {
        var type = Nullable.GetUnderlyingType(expression.Type) ?? expression.Type;
        if (type != typeof(string))
        {
            return Result.Fail(
                $"The 'contains' operator can only be used with string properties, not '{type.Name}'."
            );
        }

        var expressionToLower = Expression.Call(expression, StringToLowerMethod);
        var valueToLower = Expression.Call(value, StringToLowerMethod);
        var containsCall = Expression.Call(expressionToLower, StringContainsMethod, valueToLower);

        // Guard against null: (expression != null && expression.ToLower().Contains(value.ToLower()))
        var nullCheck = Expression.NotEqual(expression, Expression.Constant(null, typeof(string)));
        return Expression.AndAlso(nullCheck, containsCall);
    }

    private static Expression CreateEqualityExpression(
        Expression expression,
        ConstantExpression value,
        bool isEqual
    )
    {
        // For string comparisons, make them case-insensitive with null safety
        if (expression.Type == typeof(string))
        {
            var expressionToLower = Expression.Call(expression, StringToLowerMethod);
            var valueToLower = Expression.Call(value, StringToLowerMethod);

            if (isEqual)
            {
                // eq: (expression != null && expression.ToLower() == value.ToLower())
                var nullCheck = Expression.NotEqual(
                    expression,
                    Expression.Constant(null, typeof(string))
                );
                return Expression.AndAlso(
                    nullCheck,
                    Expression.Equal(expressionToLower, valueToLower)
                );
            }
            else
            {
                // ne: (expression == null || expression.ToLower() != value.ToLower())
                var isNull = Expression.Equal(
                    expression,
                    Expression.Constant(null, typeof(string))
                );
                return Expression.OrElse(
                    isNull,
                    Expression.NotEqual(expressionToLower, valueToLower)
                );
            }
        }

        // For non-string types, use standard equality
        return isEqual
            ? Expression.Equal(expression, value)
            : Expression.NotEqual(expression, value);
    }

    private static Result<Expression> EvaluateInfixExpression(
        InfixExpression exp,
        FilterEvaluationContext context
    )
    {
        if (exp.Left is PropertyPath propertyPath)
            return EvaluatePropertyPathExpression(exp, propertyPath, context);

        if (exp.Left is Identifier)
            return EvaluateIdentifierExpression(exp, context);

        if (string.IsNullOrEmpty(exp.Operator) && exp.Left is QueryLambdaExpression lambda)
            return EvaluateLambdaExpression(lambda, context);

        return EvaluateLogicalExpression(exp, context);
    }

    private static Result<Expression> EvaluateIdentifierExpression(
        InfixExpression exp,
        FilterEvaluationContext context
    )
    {
        var identifier = exp.Left.TokenLiteral();

        if (!context.PropertyMappingTree.TryGetProperty(identifier, out var propertyNode))
        {
            return Result.Fail($"Property '{identifier}' does not exist.");
        }

        var baseExpression = context.GetBaseExpression();

        var identifierProperty = Expression.Property(
            baseExpression,
            propertyNode.ActualPropertyName
        );

        if (exp.Right is NullLiteral)
        {
            return CreateNullComparison(exp, identifierProperty);
        }

        return EvaluateValueComparison(exp, identifierProperty);
    }

    private static Result<Expression> EvaluateLogicalExpression(
        InfixExpression exp,
        FilterEvaluationContext context
    )
    {
        var left = EvaluateExpression(exp.Left, context);
        if (left.IsFailed)
            return left;

        var right = EvaluateExpression(exp.Right, context);
        if (right.IsFailed)
            return right;

        return exp.Operator switch
        {
            Keywords.And => Expression.AndAlso(left.Value, right.Value),
            Keywords.Or => Expression.OrElse(left.Value, right.Value),
            _ => Result.Fail($"Unsupported logical operator '{exp.Operator}'."),
        };
    }

    private static Result<Expression> EvaluateLambdaExpression(
        QueryLambdaExpression lambdaExp,
        FilterEvaluationContext context
    )
    {
        var setupResult = SetupLambdaEvaluation(lambdaExp, context);
        if (setupResult.IsFailed)
            return Result.Fail(setupResult.Errors);

        var (collectionProperty, elementType, lambdaParameter) = setupResult.Value;

        // Enter lambda scope
        context.EnterLambdaScope(lambdaExp.Parameter, lambdaParameter, elementType);

        try
        {
            var bodyResult = EvaluateLambdaBody(lambdaExp.Body, context);
            if (bodyResult.IsFailed)
                return bodyResult;

            var lambdaExpr = Expression.Lambda(bodyResult.Value, lambdaParameter);
            return CreateLambdaLinqCall(
                lambdaExp.Function,
                collectionProperty,
                lambdaExpr,
                elementType
            );
        }
        finally
        {
            context.ExitLambdaScope();
        }
    }

    private static Result<(
        MemberExpression Collection,
        Type ElementType,
        ParameterExpression Parameter
    )> SetupLambdaEvaluation(QueryLambdaExpression lambdaExp, FilterEvaluationContext context)
    {
        var baseExpression = context.GetBaseExpression();
        var property = lambdaExp.Property;

        // When inside a lambda scope and the nested lambda's property path starts
        // with the current lambda parameter name (e.g. "o/items" where "o" is the
        // outer lambda param), strip that first segment and resolve against a
        // mapping tree built from the lambda element type instead of the root tree.
        Result<MemberExpression> collectionResult;
        if (
            context.IsInLambdaScope
            && property is PropertyPath propertyPath
            && propertyPath.Segments.Count > 1
            && propertyPath
                .Segments[0]
                .Equals(context.CurrentLambda.ParameterName, StringComparison.OrdinalIgnoreCase)
        )
        {
            var strippedSegments = propertyPath.Segments.Skip(1).ToList();
            var lambdaMappingTree = PropertyMappingTreeBuilder.BuildMappingTree(
                context.CurrentLambda.ElementType,
                context.MaxPropertyMappingDepth
            );
            var walkResult = lambdaMappingTree.WalkPropertyPath(strippedSegments, baseExpression);
            collectionResult = walkResult.IsFailed
                ? Result.Fail(walkResult.Errors)
                : Result.Ok((MemberExpression)walkResult.Value);
        }
        else
        {
            collectionResult = ResolveCollectionProperty(
                property,
                baseExpression,
                context.PropertyMappingTree
            );
        }
        if (collectionResult.IsFailed)
            return Result.Fail(collectionResult.Errors);

        var collectionProperty = collectionResult.Value;
        var elementType = GetCollectionElementType(collectionProperty.Type);

        if (elementType == null)
        {
            return Result.Fail(
                $"Property '{lambdaExp.Property.TokenLiteral()}' is not a collection."
            );
        }

        var lambdaParameter = Expression.Parameter(elementType, lambdaExp.Parameter);
        return Result.Ok((collectionProperty, elementType, lambdaParameter));
    }

    private static Expression CreateLambdaLinqCall(
        string function,
        MemberExpression collection,
        LambdaExpression lambda,
        Type elementType
    )
    {
        return function.Equals(Keywords.Any, StringComparison.OrdinalIgnoreCase)
            ? CreateAnyExpression(collection, lambda, elementType)
            : CreateAllExpression(collection, lambda, elementType);
    }

    private static Result<MemberExpression> ResolveCollectionProperty(
        QueryExpression property,
        Expression baseExpression,
        PropertyMappingTree propertyMappingTree
    )
    {
        switch (property)
        {
            case Identifier identifier:
                if (
                    !propertyMappingTree.TryGetProperty(
                        identifier.TokenLiteral(),
                        out var propertyNode
                    )
                )
                {
                    return Result.Fail($"Property '{identifier.TokenLiteral()}' does not exist.");
                }
                return Expression.Property(baseExpression, propertyNode.ActualPropertyName);

            case PropertyPath propertyPath:
                return ResolvePropertyPathForCollection(
                    propertyPath,
                    baseExpression,
                    propertyMappingTree
                );

            default:
                return Result.Fail(
                    $"Unsupported property type in lambda expression '{property.GetType().Name}'."
                );
        }
    }

    private static Result<Expression> EvaluateLambdaBody(
        QueryExpression expression,
        FilterEvaluationContext context
    )
    {
        return expression switch
        {
            InfixExpression exp when exp.Left is PropertyPath propertyPath =>
                EvaluateLambdaBodyPropertyPath(exp, propertyPath, context),

            InfixExpression exp when exp.Left is Identifier identifier =>
                EvaluateLambdaBodyIdentifier(exp, identifier, context),

            InfixExpression exp when IsNestedLambdaExpression(exp) => EvaluateLambdaExpression(
                (QueryLambdaExpression)exp.Left,
                context
            ),

            InfixExpression exp => EvaluateLambdaBodyLogicalOperator(exp, context),

            _ => Result.Fail(
                $"Unsupported expression type in lambda context '{expression.GetType().Name}'."
            ),
        };
    }

    private static bool IsNestedLambdaExpression(InfixExpression exp) =>
        string.IsNullOrEmpty(exp.Operator) && exp.Left is QueryLambdaExpression;

    private static Result<Expression> EvaluateLambdaBodyPropertyPath(
        InfixExpression exp,
        PropertyPath propertyPath,
        FilterEvaluationContext context
    )
    {
        var isLambdaParameterPath =
            propertyPath.Segments.Count > 0
            && propertyPath
                .Segments[0]
                .Equals(context.CurrentLambda.ParameterName, StringComparison.OrdinalIgnoreCase);

        return isLambdaParameterPath
            ? EvaluateLambdaPropertyPath(
                exp,
                propertyPath,
                context.CurrentLambda.Parameter,
                context.MaxPropertyMappingDepth
            )
            : EvaluatePropertyPathExpression(exp, propertyPath, context);
    }

    private static Result<Expression> EvaluateLambdaBodyIdentifier(
        InfixExpression exp,
        Identifier identifier,
        FilterEvaluationContext context
    )
    {
        var identifierName = identifier.TokenLiteral();

        if (
            identifierName.Equals(
                context.CurrentLambda.ParameterName,
                StringComparison.OrdinalIgnoreCase
            )
        )
        {
            // For primitive types (string, int, etc.), allow direct comparisons with the lambda parameter.
            if (PropertyMappingTreeBuilder.IsPrimitiveType(context.CurrentLambda.ElementType))
            {
                return EvaluateValueComparison(exp, context.CurrentLambda.Parameter);
            }

            return Result.Fail(
                $"Lambda parameter '{context.CurrentLambda.ParameterName}' cannot be used directly in comparisons for complex types."
            );
        }

        if (!context.PropertyMappingTree.TryGetProperty(identifierName, out var propertyNode))
        {
            return Result.Fail($"Property '{identifierName}' does not exist.");
        }

        var identifierProperty = Expression.Property(
            context.RootParameter,
            propertyNode.ActualPropertyName
        );
        return EvaluateValueComparison(exp, identifierProperty);
    }

    private static Result<Expression> EvaluateLambdaBodyLogicalOperator(
        InfixExpression exp,
        FilterEvaluationContext context
    )
    {
        var left = EvaluateLambdaBody(exp.Left, context);
        if (left.IsFailed)
            return left;

        var right = EvaluateLambdaBody(exp.Right, context);
        if (right.IsFailed)
            return right;

        return exp.Operator switch
        {
            Keywords.And => Expression.AndAlso(left.Value, right.Value),
            Keywords.Or => Expression.OrElse(left.Value, right.Value),
            _ => Result.Fail($"Unsupported logical operator '{exp.Operator}'."),
        };
    }

    private static Result<Expression> EvaluateLambdaPropertyPath(
        InfixExpression exp,
        PropertyPath propertyPath,
        ParameterExpression lambdaParameter,
        int maxPropertyMappingDepth
    )
    {
        // Skip the first segment (lambda parameter name) and build property path from lambda parameter
        var current = (Expression)lambdaParameter;
        var elementType = lambdaParameter.Type;

        // Build property path from lambda parameter
        var pathResult = BuildLambdaPropertyPath(
            current,
            propertyPath.Segments.Skip(1).ToList(),
            elementType,
            maxPropertyMappingDepth
        );
        if (pathResult.IsFailed)
            return pathResult;

        current = pathResult.Value;

        var finalProperty = (MemberExpression)current;

        // Handle null comparisons
        if (exp.Right is NullLiteral)
        {
            return CreateNullComparison(exp, finalProperty);
        }

        // Handle value comparisons
        return EvaluateValueComparison(exp, finalProperty);
    }

    private static Expression CreateAnyExpression(
        MemberExpression collection,
        LambdaExpression lambda,
        Type elementType
    )
    {
        var genericMethod = EnumerableAnyWithPredicate.MakeGenericMethod(elementType);
        return Expression.Call(genericMethod, collection, lambda);
    }

    private static Expression CreateAllExpression(
        MemberExpression collection,
        LambdaExpression lambda,
        Type elementType
    )
    {
        var allMethod = EnumerableAllWithPredicate.MakeGenericMethod(elementType);
        var anyMethod = EnumerableAnyWithoutPredicate.MakeGenericMethod(elementType);

        var hasElements = Expression.Call(anyMethod, collection);
        var allMatch = Expression.Call(allMethod, collection, lambda);

        return Expression.AndAlso(hasElements, allMatch);
    }

    private static Result<Expression> BuildLambdaPropertyPath(
        Expression startExpression,
        List<string> segments,
        Type elementType,
        int maxPropertyMappingDepth
    )
    {
        var mappingTree = PropertyMappingTreeBuilder.BuildMappingTree(
            elementType,
            maxPropertyMappingDepth
        );

        return mappingTree.WalkPropertyPath(segments, startExpression);
    }

    private static Type GetCollectionElementType(Type collectionType)
    {
        // Handle IEnumerable<T>
        if (collectionType.IsGenericType)
        {
            var genericArgs = collectionType.GetGenericArguments();
            if (
                genericArgs.Length == 1
                && typeof(IEnumerable<>)
                    .MakeGenericType(genericArgs[0])
                    .IsAssignableFrom(collectionType)
            )
            {
                return genericArgs[0];
            }
        }

        // Handle arrays
        if (collectionType.IsArray)
        {
            return collectionType.GetElementType();
        }

        return null;
    }

    private static Result<ConstantExpression> GetIntegerExpressionConstant(
        long value,
        Type targetType
    )
    {
        try
        {
            var type = GetNonNullableType(targetType);

            object convertedValue = type switch
            {
                Type t when t == typeof(int) => Convert.ToInt32(value),
                Type t when t == typeof(long) => value,
                Type t when t == typeof(short) => Convert.ToInt16(value),
                Type t when t == typeof(byte) => Convert.ToByte(value),
                Type t when t == typeof(uint) => Convert.ToUInt32(value),
                Type t when t == typeof(ulong) => Convert.ToUInt64(value),
                Type t when t == typeof(ushort) => Convert.ToUInt16(value),
                Type t when t == typeof(sbyte) => Convert.ToSByte(value),
                Type t when t == typeof(decimal) => Convert.ToDecimal(value),
                Type t when t == typeof(float) => Convert.ToSingle(value),
                Type t when t == typeof(double) => Convert.ToDouble(value),
                _ => throw new NotSupportedException(
                    $"Unsupported numeric type: {targetType.Name}"
                ),
            };

            return Expression.Constant(convertedValue, targetType);
        }
        catch (OverflowException)
        {
            return Result.Fail($"Value '{value}' overflows type '{targetType.Name}'.");
        }
        catch (Exception)
        {
            return Result.Fail($"Cannot convert '{value}' to type '{targetType.Name}'.");
        }
    }

    private static Result<ConstantExpression> CreateIntegerOrEnumConstant(
        long value,
        Type targetType
    )
    {
        var actualType = GetNonNullableType(targetType);

        if (actualType.IsEnum)
        {
            return ConvertIntegerToEnum(value, actualType, targetType);
        }

        return GetIntegerExpressionConstant(value, targetType);
    }

    private static Result<ConstantExpression> ConvertIntegerToEnum(
        long value,
        Type actualType,
        Type targetType
    )
    {
        try
        {
            var enumValue = Enum.ToObject(actualType, value);

            return Result.Ok(Expression.Constant(enumValue, targetType));
        }
        catch (Exception)
        {
            return Result.Fail($"Cannot convert '{value}' to enum type '{targetType.Name}'.");
        }
    }

    private static Result<ConstantExpression> CreateStringOrEnumConstant(
        string value,
        Type targetType
    )
    {
        var actualType = GetNonNullableType(targetType);

        if (actualType.IsEnum)
        {
            return ConvertStringToEnum(value, actualType, targetType);
        }

        if (actualType == typeof(Guid))
        {
            if (Guid.TryParse(value, out var guidValue))
            {
                return Result.Ok(Expression.Constant(guidValue, targetType));
            }

            return Result.Fail($"Cannot convert '{value}' to type 'Guid'.");
        }

        if (actualType != typeof(string))
        {
            return Result.Fail(
                $"Cannot use string literal for property of type '{actualType.Name}'."
            );
        }

        return Result.Ok(Expression.Constant(value, targetType));
    }

    private static Result<ConstantExpression> ConvertStringToEnum(
        string value,
        Type actualType,
        Type targetType
    )
    {
        try
        {
            var enumValue = Enum.Parse(actualType, value, true);

            return Result.Ok(Expression.Constant(enumValue, targetType));
        }
        catch (Exception)
        {
            foreach (var field in actualType.GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                var memberNameAttribute =
                    field.GetCustomAttribute<JsonStringEnumMemberNameAttribute>();

                if (
                    memberNameAttribute != null
                    && memberNameAttribute.Name.Equals(value, StringComparison.Ordinal)
                )
                {
                    return Result.Ok(Expression.Constant(field.GetValue(null), targetType));
                }
            }

            return Result.Fail(
                $"Value '{value}' is not a valid member of enum '{actualType.Name}'."
            );
        }
    }

    private static Type GetNonNullableType(Type type)
    {
        return Nullable.GetUnderlyingType(type) ?? type;
    }
}
