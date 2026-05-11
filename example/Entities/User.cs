using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

public record User
{
    public Guid Id { get; set; }
    public string Firstname { get; set; } = string.Empty;
    public string Lastname { get; set; } = string.Empty;
    public int Age { get; set; }
    public Gender Gender { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsEmailVerified { get; set; }
    public double Test { get; set; }
    public int? NullableInt { get; set; }

    public DateTimeOffset DateOfBirthUtc { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime DateOfBirthTz { get; set; }
    public User? Manager { get; set; }
    public ICollection<Address> Addresses { get; set; } = new List<Address>();
    public ICollection<string> Tags { get; set; } = new List<string>();
    public Company? Company { get; set; }
    public ICollection<Order> Orders { get; set; } = new List<Order>();
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

public record Order
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;

    public DateTimeOffset OrderDate { get; set; }
    public decimal Total { get; set; }
    public OrderStatus Status { get; set; }
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}

public record OrderItem
{
    public Guid Id { get; set; }
    public Product Product { get; set; } = new Product();
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public record Product
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Gender
{
    Male,
    Female,

    [JsonStringEnumMemberName("Alternative")]
    Other,
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OrderStatus
{
    Pending,
    Shipped,
    Delivered,
    Cancelled,
}
