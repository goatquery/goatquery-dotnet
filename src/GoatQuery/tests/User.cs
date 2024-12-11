using System.Text.Json.Serialization;

public record User
{
    public int Age { get; set; }
    public Guid UserId { get; set; }
    public string Firstname { get; set; } = string.Empty;
    public decimal? BalanceDecimal { get; set; }
    public double? BalanceDouble { get; set; }
    public float? BalanceFloat { get; set; }
    public DateTime? DateOfBirth { get; set; }
}

public sealed record CustomJsonPropertyUser : User
{
    [JsonPropertyName("last_name")]
    public string Lastname { get; set; } = string.Empty;
}