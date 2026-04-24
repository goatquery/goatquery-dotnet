using System.Text.Json;

public sealed class QueryOptions
{
    public int MaxTop { get; set; }
    public int MaxPropertyMappingDepth { get; set; } = 5;
    public JsonNamingPolicy PropertyNamingPolicy { get; set; }
}
