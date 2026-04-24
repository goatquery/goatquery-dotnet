# GoatQuery

A .NET library for parsing query parameters into LINQ expressions. Enables database-level filtering, sorting, and pagination from HTTP query strings.

> [!NOTE]
> This project only supports Entity Framework Linq currently.

## Installation

```bash
dotnet add package GoatQuery

# Or for ASP.NET Core integration please install this instead.
dotnet add package GoatQuery.AspNetCore
```

## Quick Start

```csharp
// Basic filtering
var users = dbContext.Users
    .Apply(new Query { Filter = "age gt 18 and isActive eq true" })
    .Value.Query;

// Lambda expressions for collection filtering
var usersWithLondonAddress = dbContext.Users
    .Apply(new Query { Filter = "addresses/any(x: x/city eq 'London')" })
    .Value.Query;

// Filter by primitive arrays (tags, categories, etc.)
var vipUsers = dbContext.Users
    .Apply(new Query { Filter = "tags/any(x: x eq 'vip')" })
    .Value.Results;

// Complex nested filtering
var activeUsersWithHighValueOrders = dbContext.Users
    .Apply(new Query {
        Filter = "isActive eq true and orders/any(o: o/items/any(i: i/price gt 1000))"
    })
    .Value.Query;

// ASP.NET Core integration
[HttpGet]
[EnableQuery<UserDto>(maxTop: 100)]
public IActionResult GetUsers() => Ok(dbContext.Users);
```

## Supported Syntax

```
GET /api/users?filter=age gt 18 and isActive eq true
GET /api/users?filter=addresses/any(x: x/city eq 'London')
GET /api/users?orderby=lastName asc, firstName desc
GET /api/users?orderby=company/name asc
GET /api/users?filter=tags/any(x: x eq 'premium')
GET /api/users?top=10&skip=20&count=true
GET /api/users?search=john
```

## Filtering

### Operators

- **Comparison**: `eq`, `ne`, `gt`, `gte`, `lt`, `lte`
- **Logical**: `and`, `or`
- **String**: `contains`

### Data Types

| Type | Example |
|------|---------|
| String | `'value'`, `'it\'s escaped'`, `'back\\slash'` |
| Integer | `42` |
| Float | `3.14f` |
| Decimal | `2.5m` |
| Double | `1.0d` |
| Boolean | `true`, `false` |
| DateTime | `2023-12-25T10:30:00Z` |
| DateTimeOffset | `2023-12-25T10:30:00+05:00` |
| Date | `2023-12-25` |
| GUID | `123e4567-e89b-12d3-a456-426614174000` |
| Enum | `'Active'` (string) or `1` (integer) |
| Null | `null` |

String literals support backslash escaping: `\'` for a literal single quote, `\\` for a literal backslash.

### Property Path Navigation

Access nested properties using forward slash (`/`) syntax:

```
filter=company/name eq 'TechCorp'
filter=profile/address/city eq 'London'
filter=user/addresses/any(x: x/country/name eq 'UK')
```

### Lambda Expressions

Filter collections using `any()` and `all()`:

```
// any() - true if at least one element matches
addresses/any(x: x/city eq 'London')

// all() - true if all elements match (requires non-empty collection)
addresses/all(x: x/isVerified eq true)

// Nested lambda expressions
orders/any(o: o/items/any(i: i/price gt 100))

// Primitive array filtering
tags/any(x: x eq 'premium')
scores/any(x: x gt 80)
```

### Null Safety

String comparisons (`eq`, `ne`, `contains`) are null-safe. When a string property is `null` in the database:

- `eq` and `contains` **exclude** null rows (null does not equal any value)
- `ne` **includes** null rows (null is "not equal" to any value)

### Examples

```
age gt 18
firstName eq 'John' and isActive ne false
name contains 'smith'
status eq 'Active'
createdAt gte 2023-01-01T00:00:00Z
addresses/any(x: x/city eq 'London' and x/isActive eq true)
age gt 25 and tags/any(x: x eq 'premium')
```

## Ordering

Sort by one or more properties, including nested properties:

```
GET /api/users?orderby=lastName asc
GET /api/users?orderby=lastName asc, firstName desc
GET /api/users?orderby=company/name asc
GET /api/users?orderby=company/name asc, age desc
```

Default direction is ascending when omitted.

## Property Mapping

Properties in query strings are resolved in this order:

1. `[JsonPropertyName]` attribute
2. `QueryOptions.PropertyNamingPolicy` (if configured)
3. CLR property name

Property names are matched **case-insensitively**.

### Using JsonPropertyName

```csharp
public class UserDto
{
    [JsonPropertyName("first_name")]
    public string FirstName { get; set; }

    public int Age { get; set; }
}
```

```
filter=first_name eq 'John' and age gt 18
```

### Using PropertyNamingPolicy

Apply a global naming policy instead of decorating every property:

```csharp
var options = new QueryOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
};

var result = dbContext.Users.Apply(query, options: options);
```

```
filter=first_name eq 'John' and date_of_birth gt 1990-01-01
```

## Configuration

Configure query behaviour through `QueryOptions`:

```csharp
var options = new QueryOptions
{
    MaxTop = 100,                   // Maximum allowed top value
    MaxPropertyMappingDepth = 5,    // Max depth for nested property resolution (default: 5)
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase  // Global property naming policy
};

var result = dbContext.Users.Apply(query, options: options);
```

## Search

Implement custom search logic:

```csharp
public class UserSearchBinder : ISearchBinder<User>
{
    public Expression<Func<User, bool>> Bind(string searchTerm) =>
        user => user.FirstName.Contains(searchTerm) ||
                user.LastName.Contains(searchTerm);
}

var result = users.Apply(query, new UserSearchBinder());
```

## ASP.NET Core Integration

### Action Filter

```csharp
[HttpGet]
[EnableQuery<UserDto>(maxTop: 100)]
public IActionResult GetUsers() => Ok(dbContext.Users);

// With custom depth
[EnableQuery<UserDto>(maxTop: 100, maxPropertyMappingDepth: 3)]
```

`EnableQuery` automatically resolves `JsonNamingPolicy` from your configured `JsonOptions` in DI. If you've configured a naming policy globally:

```csharp
builder.Services.Configure<JsonOptions>(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});
```

The action filter will use that policy for property resolution without additional configuration.

### Manual Processing

```csharp
[HttpGet]
public IActionResult GetUsers([FromQuery] Query query)
{
    var result = dbContext.Users.Apply(query);
    return result.IsFailed ? BadRequest(result.Errors) : Ok(result.Value.Query.ToList());
}
```

## Error Handling

Uses FluentResults pattern:

```csharp
var result = users.Apply(query);
if (result.IsFailed)
    return BadRequest(result.Errors.Select(e => e.Message));

var data = result.Value.Query.ToList();
var count = result.Value.Count;  // If Count = true
```

## Development

### Test

```bash
dotnet test ./src/GoatQuery/tests
```

### Run the example project

```bash
cd example && dotnet run
```

**Targets**: .NET Standard 2.0/2.1, .NET 6.0+
