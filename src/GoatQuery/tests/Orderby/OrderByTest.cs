using Xunit;

public sealed class OrderByTest
{
    private static readonly Dictionary<string, User> _users = new Dictionary<string, User>
    {
        ["John"] = new User { Age = 2, Firstname = "John" },
        ["Jane"] = new User { Age = 1, Firstname = "Jane" },
        ["Apple"] = new User { Age = 2, Firstname = "Apple" },
        ["Harry"] = new User { Age = 1, Firstname = "Harry" },
        ["Doe"] = new User { Age = 3, Firstname = "Doe" },
        ["Egg"] = new User { Age = 3, Firstname = "Egg" }
    };

    public static IEnumerable<object[]> Parameters()
    {
        yield return new object[]
        {
            "age desc, firstname asc",
            new[] { _users["Doe"], _users["Egg"], _users["Apple"], _users["John"], _users["Harry"], _users["Jane"] }
        };

        yield return new object[]
        {
            "age desc, firstname desc",
            new[] { _users["Egg"], _users["Doe"], _users["John"], _users["Apple"], _users["Jane"], _users["Harry"] }
        };

        yield return new object[]
        {
            "age desc",
            new[] { _users["Doe"], _users["Egg"], _users["John"], _users["Apple"], _users["Jane"], _users["Harry"] }
        };

        yield return new object[]
        {
            "Age asc",
            new[] { _users["Jane"], _users["Harry"], _users["John"], _users["Apple"], _users["Doe"], _users["Egg"] }
        };

        yield return new object[]
        {
            "age",
            new[] { _users["Jane"], _users["Harry"], _users["John"], _users["Apple"], _users["Doe"], _users["Egg"] }
        };

        yield return new object[]
        {
            "age asc",
            new[] { _users["Jane"], _users["Harry"], _users["John"], _users["Apple"], _users["Doe"], _users["Egg"] }
        };

        yield return new object[]
        {
            "Age asc",
            new[] { _users["Jane"], _users["Harry"], _users["John"], _users["Apple"], _users["Doe"], _users["Egg"] }
        };

        yield return new object[]
        {
            "aGe asc",
            new[] { _users["Jane"], _users["Harry"], _users["John"], _users["Apple"], _users["Doe"], _users["Egg"] }
        };

        yield return new object[]
        {
            "AGe asc",
            new[] { _users["Jane"], _users["Harry"], _users["John"], _users["Apple"], _users["Doe"], _users["Egg"] }
        };

        yield return new object[]
        {
            "aGE Asc",
            new[] { _users["Jane"], _users["Harry"], _users["John"], _users["Apple"], _users["Doe"], _users["Egg"] }
        };

        yield return new object[]
        {
            "age aSc",
            new[] { _users["Jane"], _users["Harry"], _users["John"], _users["Apple"], _users["Doe"], _users["Egg"] }
        };

        yield return new object[]
        {
            "",
            new[] { _users["John"], _users["Jane"], _users["Apple"], _users["Harry"], _users["Doe"], _users["Egg"] }
        };
    }

    [Theory]
    [MemberData(nameof(Parameters))]
    public void Test_OrderBy(string orderby, IEnumerable<User> expected)
    {
        var query = new Query
        {
            OrderBy = orderby
        };

        var result = _users.Values.AsQueryable().Apply(query);

        Assert.Equal(expected, result.Value.Query);
    }

    [Fact]
    public void Test_OrderBy_WithCustomJsonPropertyName()
    {
        var users = new List<CustomJsonPropertyUser>{
            new CustomJsonPropertyUser { Lastname = "John" },
            new CustomJsonPropertyUser { Lastname = "Jane" },
            new CustomJsonPropertyUser { Lastname = "Apple" },
            new CustomJsonPropertyUser { Lastname = "Harry" },
            new CustomJsonPropertyUser { Lastname = "Doe" },
            new CustomJsonPropertyUser { Lastname = "Egg" }
        }.AsQueryable();

        var query = new Query
        {
            OrderBy = "last_name asc"
        };

        var result = users.Apply(query);

        Assert.Equal(new List<CustomJsonPropertyUser>{
            new CustomJsonPropertyUser { Lastname = "Apple" },
            new CustomJsonPropertyUser { Lastname = "Doe" },
            new CustomJsonPropertyUser { Lastname = "Egg" },
            new CustomJsonPropertyUser { Lastname = "Harry" },
            new CustomJsonPropertyUser { Lastname = "Jane" },
            new CustomJsonPropertyUser { Lastname = "John" },
        }, result.Value.Query);
    }
}