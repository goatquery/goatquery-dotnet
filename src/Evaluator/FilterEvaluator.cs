using System;
using System.Linq.Expressions;

public static class FilterEvaluator
{
    public static Expression? Evaluate(QueryExpression expression, ParameterExpression parameterExpression)
    {
        switch (expression)
        {
            case InfixExpression exp:
                if (exp.Left.GetType() == typeof(Identifier))
                {
                    MemberExpression property;
                    try
                    {
                        property = Expression.Property(parameterExpression, exp.Left.TokenLiteral());
                    }
                    catch (Exception)
                    {
                        throw new GoatQueryException($"Invalid property '{exp.Left.TokenLiteral()}' within filter");
                    }

                    ConstantExpression? value = null;

                    switch (exp.Right)
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

                    switch (exp.Operator)
                    {
                        case Keywords.Eq:
                            return Expression.Equal(property, value);
                        case Keywords.Ne:
                            return Expression.NotEqual(property, value);
                        case Keywords.Contains:
                            var identifier = (Identifier)exp.Left;

                            var method = identifier.Value.GetType().GetMethod("Contains", new[] { value?.Value.GetType() });

                            return Expression.Call(property, method, value);
                    }
                }

                var left = Evaluate(exp.Left, parameterExpression);
                var right = Evaluate(exp.Right, parameterExpression);

                switch (exp.Operator)
                {
                    case Keywords.And:
                        return Expression.AndAlso(left, right);
                    case Keywords.Or:
                        return Expression.OrElse(left, right);
                }

                break;
        }

        return null;
    }
}