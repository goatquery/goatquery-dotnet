using System.Text.Json.Serialization;

public record User
{
    public int Age { get; set; }
    public Guid UserId { get; set; }
    public string Firstname { get; set; } = string.Empty;

    public string Long { get; set; } = string.Empty;
}

public sealed record CustomJsonPropertyUser : User
{
    [JsonPropertyName("last_name")]
    public string Lastname { get; set; } = string.Empty;
}