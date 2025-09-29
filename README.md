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
GET /api/users?filter=tags/any(x: x eq 'premium')
GET /api/users?top=10&skip=20&count=true
GET /api/users?search=john
```

## Filtering

### Basic Operators

- **Comparison**: `eq`, `ne`, `gt`, `gte`, `lt`, `lte`
- **Logical**: `and`, `or`
- **String**: `contains`

### Lambda Expressions

Filter collections using `any()` and `all()` with lambda expressions:

```csharp
// Users with any address in London
"addresses/any(x: x/city eq 'London')"

// Users where all addresses are verified
"addresses/all(x: x/isVerified eq true)"

// Complex nested conditions
"addresses/any(x: x/city eq 'London' and x/isActive eq true)"

// Nested collection filtering
"orders/any(o: o/items/any(i: i/price gt 100))"
```

### Property Path Navigation

Access nested properties using forward slash (`/`) syntax:

```csharp
// Navigate to nested properties
"profile/address/city eq 'London'"

// Works with collections and lambda expressions
"user/addresses/any(x: x/country/name eq 'UK')"
```

### Data Types

- String: `'value'`
- Numbers: `42`, `3.14f`, `2.5m`, `1.0d`
- Boolean: `true`, `false`
- DateTime: `2023-12-25T10:30:00Z`, `2023-12-25`
- GUID: `123e4567-e89b-12d3-a456-426614174000`
- Null: `null`

### Examples

```csharp
// Basic filtering
"age gt 18"
"firstName eq 'John' and isActive ne false"
"salary ge 50000 or department eq 'Engineering'"
"name contains 'smith'"

// Lambda expressions
"addresses/any(x: x/city eq 'London')"
"orders/all(o: o/status eq 'completed')"
"tags/any(x: x eq 'premium')"
"categories/all(x: x contains 'tech')"

// Nested properties
"profile/address/city eq 'London'"
"company/department/name contains 'Engineering'"

// Complex combinations
"age gt 25 and addresses/any(x: x/country eq 'US' and x/isActive eq true)"
"isActive eq true and tags/any(x: x eq 'premium') and scores/all(x: x gt 70)"
```

## Property Mapping

Supports `JsonPropertyName` attributes for both simple and nested properties:

```csharp
public class UserDto
{
    [JsonPropertyName("first_name")]
    public string FirstName { get; set; }

    public int Age { get; set; }  // Maps to "age"

    public List<AddressDto> Addresses { get; set; }  // Collection properties

    public ProfileDto Profile { get; set; }  // Nested objects
}

public class AddressDto
{
    [JsonPropertyName("street_address")]
    public string StreetAddress { get; set; }

    public string City { get; set; }

    [JsonPropertyName("is_verified")]
    public bool IsVerified { get; set; }
}
```

**Query Examples:**

```
filter=first_name eq 'John' and age gt 18
filter=addresses/any(x: x/street_address contains 'Main St')
filter=profile/address/city eq 'London'
```

## Advanced Features

### Lambda Expression Support

GoatQuery supports sophisticated collection filtering using lambda expressions:

#### Any/All Operations

```csharp
// any() - true if at least one element matches
"addresses/any(x: x/city eq 'London')"

// all() - true if all elements match (requires non-empty collection)
"addresses/all(x: x/isVerified eq true)"
```

#### Nested Lambda Expressions

```csharp
// Multi-level collection filtering
"orders/any(o: o/items/any(i: i/price gt 100 and i/category eq 'Electronics'))"

// Complex nested conditions
"departments/any(d: d/employees/all(e: e/isActive eq true and e/salary gt 50000))"
```

#### Lambda with Property Navigation

```csharp
// Navigate through nested objects within lambdas
"addresses/any(x: x/country/code eq 'US' and x/state/name eq 'California')"
```

#### Primitive Array Filtering

Filter arrays of primitive types (strings, numbers, etc.) directly:

```csharp
// Filter by tags (string array)
"tags/any(x: x eq 'vip')"
"tags/any(x: x contains 'premium')"
"tags/all(x: x eq 'active')"

// Filter by numeric arrays
"scores/any(x: x gt 80)"
"ratings/all(x: x ge 4.5)"

// Combined with other filters
"age gt 18 and tags/any(x: x eq 'premium')"
```

**Supported primitive types:**

- `string[]`: `tags/any(x: x eq 'premium')`
- `int[]`: `scores/any(x: x gt 90)`
- `decimal[]`: `prices/all(x: x lt 100m)`
- `DateTime[]`: `dates/any(x: x gt 2023-01-01)`
- `bool[]`: `flags/all(x: x eq true)`

This enables powerful filtering scenarios like user categorization, product tagging, content classification, and multi-criteria matching directly from query parameters.

### Null Safety

GoatQuery automatically generates null-safe expressions for property navigation:

```csharp
// Input: "profile/address/city eq 'London'"
// Generated: user.Profile != null && user.Profile.Address != null && user.Profile.Address.City == "London"
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
```

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
