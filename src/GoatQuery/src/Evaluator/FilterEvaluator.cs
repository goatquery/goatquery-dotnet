using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json.Serialization;
using FluentResults;

public static class FilterEvaluator
{
    private const string HasValuePropertyName = "HasValue";
    private const string ValuePropertyName = "Value";
    private const string DatePropertyName = "Date";

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
            return Result.Fail("Expression cannot be null");
        if (parameterExpression == null)
            return Result.Fail("Parameter expression cannot be null");
        if (propertyMappingTree == null)
            return Result.Fail("Property mapping tree cannot be null");

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
            _ => Result.Fail($"Unsupported expression type: {expression.GetType().Name}"),
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
        var result = propertyMappingTree.WalkPropertyPath(
            propertyPath.Segments,
            startExpression,
            "path"
        );
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
        var result = propertyMappingTree.WalkPropertyPath(
            propertyPath.Segments,
            baseExpression,
            "lambda expression property path"
        );
        if (result.IsFailed)
            return Result.Fail(result.Errors);
        return Result.Ok((MemberExpression)result.Value);
    }

    private static Expression CreateNullComparison(InfixExpression exp, MemberExpression property)
    {
        return exp.Operator == Keywords.Eq
            ? Expression.Equal(property, Expression.Constant(null, property.Type))
            : Expression.NotEqual(property, Expression.Constant(null, property.Type));
    }

    private static bool IsNullableDateTimeComparison(
        MemberExpression property,
        QueryExpression rightExpression
    )
    {
        return property.Type == typeof(DateTime?) && rightExpression is DateLiteral;
    }

    private static Expression CreateNullableDateTimeComparison(
        MemberExpression property,
        ConstantExpression value,
        string operatorKeyword
    )
    {
        var hasValueProperty = Expression.Property(property, HasValuePropertyName);
        var valueProperty = Expression.Property(property, ValuePropertyName);
        var dateProperty = Expression.Property(valueProperty, DatePropertyName);

        var dateComparison = CreateDateComparison(dateProperty, value, operatorKeyword);

        return operatorKeyword == Keywords.Ne
            ? Expression.OrElse(Expression.Not(hasValueProperty), dateComparison)
            : Expression.AndAlso(hasValueProperty, dateComparison);
    }

    private static Expression CreateDateComparison(
        Expression dateProperty,
        ConstantExpression value,
        string operatorKeyword
    )
    {
        return operatorKeyword switch
        {
            Keywords.Eq => Expression.Equal(dateProperty, value),
            Keywords.Ne => Expression.NotEqual(dateProperty, value),
            Keywords.Lt => Expression.LessThan(dateProperty, value),
            Keywords.Lte => Expression.LessThanOrEqual(dateProperty, value),
            Keywords.Gt => Expression.GreaterThan(dateProperty, value),
            Keywords.Gte => Expression.GreaterThanOrEqual(dateProperty, value),
            _ => throw new ArgumentException(
                $"Unsupported operator for date comparison: {operatorKeyword}"
            ),
        };
    }

    private static Result<Expression> EvaluateValueComparison(
        InfixExpression exp,
        MemberExpression property
    )
    {
        var valueResult = CreateConstantExpression(exp.Right, property);
        if (valueResult.IsFailed)
            return Result.Fail(valueResult.Errors);

        var (value, updatedProperty) = valueResult.Value;

        if (IsNullableDateTimeComparison(updatedProperty, exp.Right))
        {
            return CreateNullableDateTimeComparison(updatedProperty, value, exp.Operator);
        }

        return CreateComparisonExpression(exp.Operator, updatedProperty, value);
    }

    private static Result<Expression> EvaluateValueComparison(
        InfixExpression exp,
        Expression expression
    )
    {
        var valueResult = CreateConstantExpression(exp.Right, expression);
        if (valueResult.IsFailed)
            return Result.Fail(valueResult.Errors);

        return CreateComparisonExpression(exp.Operator, expression, valueResult.Value);
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
            Keywords.Lt => Expression.LessThan(expression, value),
            Keywords.Lte => Expression.LessThanOrEqual(expression, value),
            Keywords.Gt => Expression.GreaterThan(expression, value),
            Keywords.Gte => Expression.GreaterThanOrEqual(expression, value),
            _ => Result.Fail($"Unsupported operator: {operatorKeyword}"),
        };
    }

    private static Result<Expression> CreateComparisonExpression(
        string operatorKeyword,
        MemberExpression property,
        ConstantExpression value
    )
    {
        return CreateComparisonExpression(operatorKeyword, (Expression)property, value);
    }

    private static Result<ConstantExpression> CreateConstantExpression(
        QueryExpression literal,
        Expression expression
    )
    {
        return literal switch
        {
            IntegerLiteral intLit => CreateIntegerOrEnumConstant(intLit.Value, expression.Type),
            LongLiteral longLit => CreateLongConstant(longLit.Value, expression.Type),
            DateLiteral dateLit => Result.Ok(CreateDateConstant(dateLit, expression.Type)),
            GuidLiteral guidLit => Result.Ok(Expression.Constant(guidLit.Value, expression.Type)),
            DecimalLiteral decLit => Result.Ok(Expression.Constant(decLit.Value, expression.Type)),
            FloatLiteral floatLit => Result.Ok(
                Expression.Constant(floatLit.Value, expression.Type)
            ),
            DoubleLiteral dblLit => Result.Ok(Expression.Constant(dblLit.Value, expression.Type)),
            StringLiteral strLit => CreateStringOrEnumConstant(strLit.Value, expression.Type),
            DateTimeLiteral dtLit => Result.Ok(CreateDateTimeConstant(dtLit, expression.Type)),
            BooleanLiteral boolLit => Result.Ok(
                Expression.Constant(boolLit.Value, expression.Type)
            ),
            NullLiteral _ => Result.Ok(Expression.Constant(null, expression.Type)),
            _ => Result.Fail($"Unsupported literal type: {literal.GetType().Name}"),
        };
    }

    private static Result<(
        ConstantExpression Value,
        MemberExpression Property
    )> CreateConstantExpression(QueryExpression literal, MemberExpression property)
    {
        var constantResult = CreateConstantExpression(literal, (Expression)property);
        if (constantResult.IsFailed)
            return Result.Fail(constantResult.Errors);

        return Result.Ok((constantResult.Value, property));
    }

    private static ConstantExpression CreateDateConstant(DateLiteral dateLiteral, Type targetType)
    {
        var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (underlyingType == typeof(DateTimeOffset))
        {
            var value = new DateTimeOffset(dateLiteral.Value.Date, TimeSpan.Zero);
            return Expression.Constant(value, targetType);
        }

        return Expression.Constant(dateLiteral.Value.Date, targetType);
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

    private static Expression CreateContainsExpression(
        Expression expression,
        ConstantExpression value
    )
    {
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
            return Result.Fail($"Invalid property '{identifier}' within filter");
        }

        var baseExpression = context.GetBaseExpression();

        var identifierProperty = Expression.Property(
            baseExpression,
            propertyNode.ActualPropertyName
        );
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
            _ => Result.Fail($"Unsupported logical operator: {exp.Operator}"),
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
            var walkResult = lambdaMappingTree.WalkPropertyPath(
                strippedSegments,
                baseExpression,
                "lambda expression property path"
            );
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
                $"Property '{lambdaExp.Property.TokenLiteral()}' is not a collection"
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
                    return Result.Fail(
                        $"Invalid property '{identifier.TokenLiteral()}' in lambda expression"
                    );
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
                    $"Unsupported property type in lambda expression: {property.GetType().Name}"
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
                $"Unsupported expression type in lambda context: {expression.GetType().Name}"
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
            // For primitive types (string, int, etc.), allow direct comparisons with the lambda parameter
            if (PropertyMappingTreeBuilder.IsPrimitiveType(context.CurrentLambda.ElementType))
            {
                return EvaluateValueComparison(exp, context.CurrentLambda.Parameter);
            }

            return Result.Fail(
                $"Lambda parameter '{context.CurrentLambda.ParameterName}' cannot be used directly in comparisons for complex types"
            );
        }

        if (!context.PropertyMappingTree.TryGetProperty(identifierName, out var propertyNode))
        {
            return Result.Fail($"Invalid property '{identifierName}' within filter");
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
            _ => Result.Fail($"Unsupported logical operator: {exp.Operator}"),
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
            return exp.Operator == Keywords.Eq
                ? Expression.Equal(finalProperty, Expression.Constant(null, finalProperty.Type))
                : Expression.NotEqual(finalProperty, Expression.Constant(null, finalProperty.Type));
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

        return mappingTree.WalkPropertyPath(segments, startExpression, "lambda property path");
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
        int value,
        Type targetType
    )
    {
        try
        {
            var type = GetNonNullableType(targetType);

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
                _ => throw new NotSupportedException(
                    $"Unsupported numeric type: {targetType.Name}"
                ),
            };

            return Expression.Constant(convertedValue, targetType);
        }
        catch (OverflowException)
        {
            return Result.Fail($"Value {value} is too large for type {targetType.Name}");
        }
        catch (Exception)
        {
            return Result.Fail($"Error converting {value} to {targetType.Name}");
        }
    }

    private static Result<ConstantExpression> CreateLongConstant(long value, Type targetType)
    {
        try
        {
            var type = GetNonNullableType(targetType);

            object convertedValue = type switch
            {
                Type t when t == typeof(long) => value,
                Type t when t == typeof(int) => Convert.ToInt32(value),
                Type t when t == typeof(short) => Convert.ToInt16(value),
                Type t when t == typeof(byte) => Convert.ToByte(value),
                Type t when t == typeof(uint) => Convert.ToUInt32(value),
                Type t when t == typeof(ulong) => Convert.ToUInt64(value),
                Type t when t == typeof(ushort) => Convert.ToUInt16(value),
                Type t when t == typeof(sbyte) => Convert.ToSByte(value),
                _ => throw new NotSupportedException(
                    $"Unsupported numeric type: {targetType.Name}"
                ),
            };

            return Expression.Constant(convertedValue, targetType);
        }
        catch (OverflowException)
        {
            return Result.Fail($"Value {value} is too large for type {targetType.Name}");
        }
        catch (Exception)
        {
            return Result.Fail($"Error converting {value} to {targetType.Name}");
        }
    }

    private static Result<ConstantExpression> CreateIntegerOrEnumConstant(
        int value,
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
        int value,
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
            return Result.Fail($"Error converting {value} to enum type {targetType.Name}");
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

            return Result.Fail($"Value '{value}' is not a valid member of enum {actualType.Name}");
        }
    }

    private static Type GetNonNullableType(Type type)
    {
        return Nullable.GetUnderlyingType(type) ?? type;
    }
}
