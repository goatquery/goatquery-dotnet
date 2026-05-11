namespace GoatQuery;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentResults;

internal sealed class PropertyMappingTree
{
    private readonly Dictionary<string, PropertyMappingNode> _properties;

    internal PropertyMappingTree()
    {
        _properties = new Dictionary<string, PropertyMappingNode>(StringComparer.OrdinalIgnoreCase);
    }

    public bool TryGetProperty(string jsonPropertyName, out PropertyMappingNode node)
    {
        if (string.IsNullOrEmpty(jsonPropertyName))
        {
            node = null;
            return false;
        }

        return _properties.TryGetValue(jsonPropertyName, out node);
    }

    internal void AddProperty(string jsonPropertyName, PropertyMappingNode node)
    {
        _properties[jsonPropertyName] = node;
    }

    public Result<Expression> WalkPropertyPath(
        IReadOnlyList<string> segments,
        Expression startExpression
    )
    {
        var current = startExpression;
        var currentMappingTree = this;

        for (int i = 0; i < segments.Count; i++)
        {
            var segment = segments[i];

            if (!currentMappingTree.TryGetProperty(segment, out var propertyNode))
                return Result.Fail($"Property '{segment}' does not exist.");

            current = Expression.Property(current, propertyNode.ActualPropertyName);

            if (i < segments.Count - 1)
            {
                if (!propertyNode.HasNestedMapping)
                    return Result.Fail($"Property '{segment}' does not support nested navigation.");

                currentMappingTree = propertyNode.NestedMapping;
            }
        }

        return Result.Ok(current);
    }
}

internal sealed class PropertyMappingNode
{
    public string JsonPropertyName { get; }
    public string ActualPropertyName { get; }
    public Type PropertyType { get; }
    public PropertyMappingTree NestedMapping { get; internal set; }
    public Type CollectionElementType { get; }

    internal PropertyMappingNode(
        string jsonPropertyName,
        string actualPropertyName,
        Type propertyType,
        Type collectionElementType = null
    )
    {
        JsonPropertyName =
            jsonPropertyName ?? throw new ArgumentNullException(nameof(jsonPropertyName));
        ActualPropertyName =
            actualPropertyName ?? throw new ArgumentNullException(nameof(actualPropertyName));
        PropertyType = propertyType ?? throw new ArgumentNullException(nameof(propertyType));
        CollectionElementType = collectionElementType;
    }

    public bool HasNestedMapping => NestedMapping != null;
}

internal static class PropertyMappingTreeBuilder
{
    private static readonly HashSet<Type> PrimitiveTypes = new HashSet<Type>
    {
        typeof(string),
        typeof(decimal),
        typeof(DateTime),
        typeof(DateTimeOffset),
        typeof(TimeSpan),
        typeof(Guid),
    };

    public static PropertyMappingTree BuildMappingTree<T>(
        int maxDepth,
        JsonNamingPolicy namingPolicy = null
    )
    {
        return BuildMappingTree(typeof(T), maxDepth, namingPolicy);
    }

    public static PropertyMappingTree BuildMappingTree(
        Type type,
        int maxDepth,
        JsonNamingPolicy namingPolicy = null
    )
    {
        if (type == null)
            throw new ArgumentNullException(nameof(type));
        if (maxDepth <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(maxDepth),
                "Max depth must be greater than 0"
            );

        return BuildMappingTreeInternal(
            type,
            maxDepth,
            currentDepth: 0,
            new List<Type>(),
            namingPolicy
        );
    }

    private static PropertyMappingTree BuildMappingTreeInternal(
        Type type,
        int maxDepth,
        int currentDepth,
        List<Type> typePath,
        JsonNamingPolicy namingPolicy
    )
    {
        var tree = new PropertyMappingTree();

        if (currentDepth >= maxDepth)
            return tree;

        typePath.Add(type);
        try
        {
            BuildPropertiesForTree(tree, type, maxDepth, currentDepth, typePath, namingPolicy);
        }
        finally
        {
            typePath.RemoveAt(typePath.Count - 1);
        }

        return tree;
    }

    private static void BuildPropertiesForTree(
        PropertyMappingTree tree,
        Type type,
        int maxDepth,
        int currentDepth,
        List<Type> typePath,
        JsonNamingPolicy namingPolicy
    )
    {
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            var node = CreatePropertyNode(property, namingPolicy);
            var typeToProcess = node.CollectionElementType ?? node.PropertyType;

            if (
                ShouldCreateNestedMapping(typeToProcess)
                && CanNavigateToType(typeToProcess, typePath, maxDepth)
            )
            {
                node.NestedMapping = BuildMappingTreeInternal(
                    typeToProcess,
                    maxDepth,
                    currentDepth + 1,
                    new List<Type>(typePath),
                    namingPolicy
                );
            }

            tree.AddProperty(node.JsonPropertyName, node);
        }
    }

    private static PropertyMappingNode CreatePropertyNode(
        PropertyInfo property,
        JsonNamingPolicy namingPolicy
    )
    {
        var jsonPropertyName = GetJsonPropertyName(property, namingPolicy);
        var elementType = GetCollectionElementType(property.PropertyType);

        return new PropertyMappingNode(
            jsonPropertyName,
            property.Name,
            property.PropertyType,
            elementType
        );
    }

    private static bool CanNavigateToType(Type type, List<Type> typePath, int maxDepth)
    {
        var typeCount = typePath.Count(t => t == type);
        return typeCount < maxDepth;
    }

    private static string GetJsonPropertyName(PropertyInfo property, JsonNamingPolicy namingPolicy)
    {
        return property.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name
            ?? namingPolicy?.ConvertName(property.Name)
            ?? property.Name;
    }

    internal static Type GetCollectionElementType(Type type)
    {
        if (type.IsArray)
            return type.GetElementType();

        if (type.IsGenericType && type.GetGenericArguments().Length == 1)
        {
            var elementType = type.GetGenericArguments()[0];
            var enumerableType = typeof(IEnumerable<>).MakeGenericType(elementType);

            if (enumerableType.IsAssignableFrom(type))
                return elementType;
        }

        return null;
    }

    private static bool ShouldCreateNestedMapping(Type type)
    {
        return !IsPrimitiveType(type)
            && type != typeof(object)
            && !type.IsAbstract
            && !type.IsInterface;
    }

    internal static bool IsPrimitiveType(Type type)
    {
        if (type.IsPrimitive || PrimitiveTypes.Contains(type))
            return true;

        var underlyingType = Nullable.GetUnderlyingType(type);
        return underlyingType != null
            && (underlyingType.IsPrimitive || PrimitiveTypes.Contains(underlyingType));
    }
}
