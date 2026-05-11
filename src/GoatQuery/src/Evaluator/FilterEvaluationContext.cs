namespace GoatQuery;

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

internal sealed class FilterEvaluationContext
{
    public ParameterExpression RootParameter { get; }
    public PropertyMappingTree PropertyMappingTree { get; }
    public int MaxPropertyMappingDepth { get; }
    public Stack<LambdaScope> LambdaScopes { get; } = new Stack<LambdaScope>();

    public FilterEvaluationContext(
        ParameterExpression rootParameter,
        PropertyMappingTree propertyMappingTree,
        int maxPropertyMappingDepth
    )
    {
        RootParameter = rootParameter;
        PropertyMappingTree = propertyMappingTree;
        MaxPropertyMappingDepth = maxPropertyMappingDepth;
    }

    public bool IsInLambdaScope => LambdaScopes.Count > 0;
    public LambdaScope CurrentLambda => LambdaScopes.Peek();

    public Expression GetBaseExpression()
    {
        return IsInLambdaScope ? CurrentLambda.Parameter : RootParameter;
    }

    public void EnterLambdaScope(
        string parameterName,
        ParameterExpression parameter,
        Type elementType
    )
    {
        LambdaScopes.Push(new LambdaScope(parameterName, parameter, elementType));
    }

    public void ExitLambdaScope() => LambdaScopes.Pop();
}

internal sealed class LambdaScope
{
    public string ParameterName { get; }
    public ParameterExpression Parameter { get; }
    public Type ElementType { get; }

    public LambdaScope(string parameterName, ParameterExpression parameter, Type elementType)
    {
        ParameterName = parameterName;
        Parameter = parameter;
        ElementType = elementType;
    }
}
