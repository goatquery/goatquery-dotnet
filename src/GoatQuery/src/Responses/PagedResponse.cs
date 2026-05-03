using System.Collections.Generic;
using System.Text.Json.Serialization;

/// <summary>
/// A JSON-serializable response containing a page of results and an optional total count.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public sealed class PagedResponse<T>
{
    public PagedResponse(IEnumerable<T> data, int? count = null)
    {
        Value = data;
        Count = count;
    }

    public PagedResponse()
    {
        Value = new List<T>();
    }

    /// <summary>
    /// Total count of matching items before pagination. Omitted from JSON when <c>null</c>.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Count { get; set; }

    /// <summary>The page of results.</summary>
    public IEnumerable<T> Value { get; set; }
}
