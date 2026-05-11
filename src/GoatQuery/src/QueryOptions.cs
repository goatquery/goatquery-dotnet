namespace GoatQuery;

using System.Text.Json;

/// <summary>
/// Configuration options that control query processing behaviour.
/// </summary>
public sealed class QueryOptions
{
    /// <summary>
    /// Maximum allowed value for <see cref="Query.Top"/>.
    /// When a request supplies a <c>Top</c> greater than this value, <c>Apply</c> returns an error.
    /// When <c>Top</c> is not supplied and <c>MaxTop</c> is greater than zero, <c>MaxTop</c> is
    /// applied as a default limit. A value of <c>0</c> disables the limit.
    /// </summary>
    public int MaxTop { get; set; }

    /// <summary>
    /// Maximum depth for nested property resolution. Prevents infinite recursion on
    /// self-referencing types. Defaults to <c>5</c>.
    /// </summary>
    public int MaxPropertyMappingDepth { get; set; } = 5;

    /// <summary>
    /// Global naming policy used to map query-string property names to CLR property names.
    /// When set, property names are resolved through this policy before falling back to the
    /// CLR name. <see cref="System.Text.Json.Serialization.JsonPropertyNameAttribute"/> takes
    /// precedence over this policy.
    /// </summary>
    public JsonNamingPolicy PropertyNamingPolicy { get; set; }
}
