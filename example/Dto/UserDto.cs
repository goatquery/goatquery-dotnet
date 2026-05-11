using System.Text.Json.Serialization;

public record UserDto
{
    public Guid Id { get; set; }

    [JsonPropertyName("first_name")]
    public string Firstname { get; set; } = string.Empty;
    public string Lastname { get; set; } = string.Empty;
    public int Age { get; set; }
    public Gender Gender { get; set; }
    public bool IsEmailVerified { get; set; }
    public double Test { get; set; }
    public int? NullableInt { get; set; }
    public DateTimeOffset DateOfBirthUtc { get; set; }
    public DateTime DateOfBirthTz { get; set; }
    public User? Manager { get; set; }
    public IEnumerable<AddressDto> Addresses { get; set; } = Array.Empty<AddressDto>();
    public ICollection<string> Tags { get; set; } = Array.Empty<string>();
    public CompanyDto? Company { get; set; }
    public IEnumerable<OrderDto> Orders { get; set; } = Array.Empty<OrderDto>();
}

public record AddressDto
{
    public string AddressLine1 { get; set; } = string.Empty;
    public CityDto City { get; set; } = new CityDto();
}

public record CityDto
{
    public string Name { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}

public record CompanyDto
{
    public string Name { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
}

public record OrderDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTimeOffset OrderDate { get; set; }
    public decimal Total { get; set; }
    public OrderStatus Status { get; set; }
    public IEnumerable<OrderItemDto> Items { get; set; } = Array.Empty<OrderItemDto>();
}

public record OrderItemDto
{
    public ProductDto Product { get; set; } = new ProductDto();
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public record ProductDto
{
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Price { get; set; }
}
