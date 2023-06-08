using System.Text.Json.Serialization;

public class PagedResponse<T>
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

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Count { get; set; }

    public IEnumerable<T> Value { get; set; }
}