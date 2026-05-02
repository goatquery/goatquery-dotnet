using System.Text.Json.Serialization;

public record User
{
    public Guid Id { get; set; }
    public int Age { get; set; }
    public Gender? Gender { get; set; }
    public Status Status { get; set; }
    public string Firstname { get; set; } = string.Empty;
    public decimal? BalanceDecimal { get; set; }
    public double? BalanceDouble { get; set; }
    public float? BalanceFloat { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public bool IsEmailVerified { get; set; }
    public long? LargeNumber { get; set; }
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

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Gender
{
    Male,
    Female,

    [JsonStringEnumMemberName("Alternative")]
    Other,
}

public enum Status
{
    Active,
    Inactive,
}

public record CamelCaseUser
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public int Age { get; set; }
    public CamelCaseCompany? Company { get; set; }
}

public record CamelCaseCompany
{
    public string CompanyName { get; set; } = string.Empty;
}

public record NullableStringUser
{
    public string? Firstname { get; set; }
    public int Age { get; set; }
}

public record DepthTestNode
{
    public string Name { get; set; } = string.Empty;
    public DepthTestChild? Child { get; set; }
}

public record DepthTestChild
{
    public string Value { get; set; } = string.Empty;
    public IEnumerable<DepthTestGrandchild> Items { get; set; } =
        Array.Empty<DepthTestGrandchild>();
}

public record DepthTestGrandchild
{
    public string Label { get; set; } = string.Empty;
    public DepthTestGreatGrandchild? Deep { get; set; }
}

public record DepthTestGreatGrandchild
{
    public string Detail { get; set; } = string.Empty;
}

public record DateTimeOffsetUser
{
    public string Name { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
