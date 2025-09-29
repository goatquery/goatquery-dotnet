using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;

public sealed class PropertyMappingTree
{
    public IReadOnlyDictionary<string, PropertyMappingNode> Properties { get; }
    public Type SourceType { get; }

    internal PropertyMappingTree(Type sourceType)
    {
        SourceType = sourceType ?? throw new ArgumentNullException(nameof(sourceType));
        Properties = new Dictionary<string, PropertyMappingNode>(StringComparer.OrdinalIgnoreCase);
    }

    public bool TryGetProperty(string jsonPropertyName, out PropertyMappingNode node)
    {
        if (string.IsNullOrEmpty(jsonPropertyName))
        {
            node = null;
            return false;
        }

        return ((Dictionary<string, PropertyMappingNode>)Properties).TryGetValue(jsonPropertyName, out node);
    }

    internal void AddProperty(string jsonPropertyName, PropertyMappingNode node)
    {
        ((Dictionary<string, PropertyMappingNode>)Properties)[jsonPropertyName] = node;
    }
}

public sealed class PropertyMappingNode
{
    public string JsonPropertyName { get; }
    public string ActualPropertyName { get; }
    public Type PropertyType { get; }
    public PropertyMappingTree NestedMapping { get; internal set; }
    public bool IsCollection { get; }
    public Type CollectionElementType { get; }

    internal PropertyMappingNode(
        string jsonPropertyName,
        string actualPropertyName,
        Type propertyType,
        bool isCollection = false,
        Type collectionElementType = null)
    {
        JsonPropertyName = jsonPropertyName ?? throw new ArgumentNullException(nameof(jsonPropertyName));
        ActualPropertyName = actualPropertyName ?? throw new ArgumentNullException(nameof(actualPropertyName));
        PropertyType = propertyType ?? throw new ArgumentNullException(nameof(propertyType));
        IsCollection = isCollection;
        CollectionElementType = collectionElementType;
    }

    public bool HasNestedMapping => NestedMapping != null;
}

public static class PropertyMappingTreeBuilder
{
    private static readonly HashSet<Type> PrimitiveTypes = new HashSet<Type>
    {
        typeof(string), typeof(decimal), typeof(DateTime),
        typeof(DateTimeOffset), typeof(TimeSpan), typeof(Guid)
    };

    public static PropertyMappingTree BuildMappingTree<T>(int maxDepth)
    {
        return BuildMappingTree(typeof(T), maxDepth);
    }

    public static PropertyMappingTree BuildMappingTree(Type type, int maxDepth)
    {
        if (type == null) throw new ArgumentNullException(nameof(type));
        if (maxDepth <= 0) throw new ArgumentOutOfRangeException(nameof(maxDepth), "Max depth must be greater than 0");

        return BuildMappingTreeInternal(type, maxDepth, currentDepth: 0, new List<Type>());
    }

    private static PropertyMappingTree BuildMappingTreeInternal(Type type, int maxDepth, int currentDepth, List<Type> typePath)
    {
        var tree = new PropertyMappingTree(type);

        if (currentDepth >= maxDepth)
            return tree;

        typePath.Add(type);
        try
        {
            BuildPropertiesForTree(tree, type, maxDepth, currentDepth, typePath);
        }
        finally
        {
            typePath.RemoveAt(typePath.Count - 1);
        }

        return tree;
    }

    private static void BuildPropertiesForTree(PropertyMappingTree tree, Type type, int maxDepth, int currentDepth, List<Type> typePath)
    {
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            var node = CreatePropertyNode(property);
            var typeToProcess = node.CollectionElementType ?? node.PropertyType;

            if (ShouldCreateNestedMapping(typeToProcess) && CanNavigateToType(typeToProcess, typePath, maxDepth))
            {
                node.NestedMapping = BuildMappingTreeInternal(
                    typeToProcess,
                    maxDepth,
                    currentDepth + 1,
                    new List<Type>(typePath));
            }

            tree.AddProperty(node.JsonPropertyName, node);
        }
    }

    private static PropertyMappingNode CreatePropertyNode(PropertyInfo property)
    {
        var jsonPropertyName = GetJsonPropertyName(property);
        var (isCollection, elementType) = GetCollectionInfo(property.PropertyType);

        return new PropertyMappingNode(
            jsonPropertyName,
            property.Name,
            property.PropertyType,
            isCollection,
            elementType);
    }

    private static bool CanNavigateToType(Type type, List<Type> typePath, int maxDepth)
    {
        var typeCount = typePath.Count(t => t == type);
        return typeCount < maxDepth;
    }

    private static string GetJsonPropertyName(PropertyInfo property)
    {
        return property.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? property.Name;
    }

    private static (bool IsCollection, Type ElementType) GetCollectionInfo(Type type)
    {
        if (type.IsArray)
            return (true, type.GetElementType());

        if (type.IsGenericType && type.GetGenericArguments().Length == 1)
        {
            var elementType = type.GetGenericArguments()[0];
            var enumerableType = typeof(IEnumerable<>).MakeGenericType(elementType);

            if (enumerableType.IsAssignableFrom(type))
                return (true, elementType);
        }

        return (false, null);
    }

    private static bool ShouldCreateNestedMapping(Type type)
    {
        return !IsPrimitiveType(type) &&
               type != typeof(object) &&
               !type.IsAbstract &&
               !type.IsInterface;
    }

    private static bool IsPrimitiveType(Type type)
    {
        if (type.IsPrimitive || PrimitiveTypes.Contains(type))
            return true;

        var underlyingType = Nullable.GetUnderlyingType(type);
        return underlyingType != null && (underlyingType.IsPrimitive || PrimitiveTypes.Contains(underlyingType));
    }
}