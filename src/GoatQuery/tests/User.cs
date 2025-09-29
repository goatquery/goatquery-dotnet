using System.Text.Json.Serialization;

public record User
{
    public Guid Id { get; set; }
    public int Age { get; set; }
    public string Firstname { get; set; } = string.Empty;
    public decimal? BalanceDecimal { get; set; }
    public double? BalanceDouble { get; set; }
    public float? BalanceFloat { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public bool IsEmailVerified { get; set; }
    public Company? Company { get; set; }

    public Guid? ManagerId { get; set; }
    public User? Manager { get; set; }
    public IEnumerable<Address> Addresses { get; set; } = Array.Empty<Address>();
    public IEnumerable<string> Tags { get; set; } = Array.Empty<string>();
}

public record Address
{
    public Guid Id { get; set; }
    public City City { get; set; } = new City();
    public string AddressLine1 { get; set; } = string.Empty;
}

public record City
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}

public record Company
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
}

public sealed record CustomJsonPropertyUser : User
{
    [JsonPropertyName("last_name")]
    public string Lastname { get; set; } = string.Empty;
}