using System.Text.Json.Serialization;

public record User
{
    public int Age { get; set; }
    public string Firstname { get; set; } = string.Empty;
}

public sealed record CustomJsonPropertyUser : User
{
    [JsonPropertyName("last_name")]
    public string Lastname { get; set; } = string.Empty;
}