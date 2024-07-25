using System.Text.Json.Serialization;

public record UserDto
{
    public Guid Id { get; set; }

    [JsonPropertyName("first_name")]
    public string Firstname { get; set; } = string.Empty;
    public string Lastname { get; set; } = string.Empty;
    public int Age { get; set; }
    public double Test { get; set; }
}