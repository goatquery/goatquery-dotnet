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
        ["Egg"] = new User { Age = 3, Firstname = "Egg" },
    };

    public static IEnumerable<object[]> Parameters()
    {
        yield return new object[]
        {
            "age desc, firstname asc",
            new[]
            {
                _users["Doe"],
                _users["Egg"],
                _users["Apple"],
                _users["John"],
                _users["Harry"],
                _users["Jane"],
            },
        };

        yield return new object[]
        {
            "age desc, firstname desc",
            new[]
            {
                _users["Egg"],
                _users["Doe"],
                _users["John"],
                _users["Apple"],
                _users["Jane"],
                _users["Harry"],
            },
        };

        yield return new object[]
        {
            "age desc",
            new[]
            {
                _users["Doe"],
                _users["Egg"],
                _users["John"],
                _users["Apple"],
                _users["Jane"],
                _users["Harry"],
            },
        };

        yield return new object[]
        {
            "Age asc",
            new[]
            {
                _users["Jane"],
                _users["Harry"],
                _users["John"],
                _users["Apple"],
                _users["Doe"],
                _users["Egg"],
            },
        };

        yield return new object[]
        {
            "age",
            new[]
            {
                _users["Jane"],
                _users["Harry"],
                _users["John"],
                _users["Apple"],
                _users["Doe"],
                _users["Egg"],
            },
        };

        yield return new object[]
        {
            "age asc",
            new[]
            {
                _users["Jane"],
                _users["Harry"],
                _users["John"],
                _users["Apple"],
                _users["Doe"],
                _users["Egg"],
            },
        };

        yield return new object[]
        {
            "Age asc",
            new[]
            {
                _users["Jane"],
                _users["Harry"],
                _users["John"],
                _users["Apple"],
                _users["Doe"],
                _users["Egg"],
            },
        };

        yield return new object[]
        {
            "aGe asc",
            new[]
            {
                _users["Jane"],
                _users["Harry"],
                _users["John"],
                _users["Apple"],
                _users["Doe"],
                _users["Egg"],
            },
        };

        yield return new object[]
        {
            "AGe asc",
            new[]
            {
                _users["Jane"],
                _users["Harry"],
                _users["John"],
                _users["Apple"],
                _users["Doe"],
                _users["Egg"],
            },
        };

        yield return new object[]
        {
            "aGE Asc",
            new[]
            {
                _users["Jane"],
                _users["Harry"],
                _users["John"],
                _users["Apple"],
                _users["Doe"],
                _users["Egg"],
            },
        };

        yield return new object[]
        {
            "age aSc",
            new[]
            {
                _users["Jane"],
                _users["Harry"],
                _users["John"],
                _users["Apple"],
                _users["Doe"],
                _users["Egg"],
            },
        };

        yield return new object[]
        {
            "",
            new[]
            {
                _users["John"],
                _users["Jane"],
                _users["Apple"],
                _users["Harry"],
                _users["Doe"],
                _users["Egg"],
            },
        };
    }

    [Theory]
    [MemberData(nameof(Parameters))]
    public void Test_OrderBy(string orderby, IEnumerable<User> expected)
    {
        var query = new Query { OrderBy = orderby };

        var result = _users.Values.AsQueryable().Apply(query);

        Assert.Equal(expected, result.Value.Query);
    }

    [Fact]
    public void Test_OrderBy_WithCustomJsonPropertyName()
    {
        var users = new List<CustomJsonPropertyUser>
        {
            new CustomJsonPropertyUser { Lastname = "John" },
            new CustomJsonPropertyUser { Lastname = "Jane" },
            new CustomJsonPropertyUser { Lastname = "Apple" },
            new CustomJsonPropertyUser { Lastname = "Harry" },
            new CustomJsonPropertyUser { Lastname = "Doe" },
            new CustomJsonPropertyUser { Lastname = "Egg" },
        }.AsQueryable();

        var query = new Query { OrderBy = "last_name asc" };

        var result = users.Apply(query);

        Assert.Equal(
            new List<CustomJsonPropertyUser>
            {
                new CustomJsonPropertyUser { Lastname = "Apple" },
                new CustomJsonPropertyUser { Lastname = "Doe" },
                new CustomJsonPropertyUser { Lastname = "Egg" },
                new CustomJsonPropertyUser { Lastname = "Harry" },
                new CustomJsonPropertyUser { Lastname = "Jane" },
                new CustomJsonPropertyUser { Lastname = "John" },
            },
            result.Value.Query
        );
    }

    [Fact]
    public void Test_OrderBy_NestedProperty()
    {
        var users = new List<User>
        {
            new User
            {
                Firstname = "A",
                Company = new Company { Name = "Zebra" },
            },
            new User
            {
                Firstname = "B",
                Company = new Company { Name = "Alpha" },
            },
            new User
            {
                Firstname = "C",
                Company = new Company { Name = "Middle" },
            },
        }.AsQueryable();

        var query = new Query { OrderBy = "company/name asc" };
        var result = users.Apply(query);

        Assert.True(result.IsSuccess);
        var ordered = result.Value.Query.ToList();
        Assert.Equal("B", ordered[0].Firstname); // Alpha
        Assert.Equal("C", ordered[1].Firstname); // Middle
        Assert.Equal("A", ordered[2].Firstname); // Zebra
    }

    [Fact]
    public void Test_OrderBy_NestedProperty_Descending()
    {
        var users = new List<User>
        {
            new User
            {
                Firstname = "A",
                Company = new Company { Name = "Zebra" },
            },
            new User
            {
                Firstname = "B",
                Company = new Company { Name = "Alpha" },
            },
            new User
            {
                Firstname = "C",
                Company = new Company { Name = "Middle" },
            },
        }.AsQueryable();

        var query = new Query { OrderBy = "company/name desc" };
        var result = users.Apply(query);

        Assert.True(result.IsSuccess);
        var ordered = result.Value.Query.ToList();
        Assert.Equal("A", ordered[0].Firstname); // Zebra
        Assert.Equal("C", ordered[1].Firstname); // Middle
        Assert.Equal("B", ordered[2].Firstname); // Alpha
    }

    [Fact]
    public void Test_OrderBy_NestedProperty_WithFlatProperty()
    {
        var users = new List<User>
        {
            new User
            {
                Firstname = "A",
                Age = 30,
                Company = new Company { Name = "Zebra" },
            },
            new User
            {
                Firstname = "B",
                Age = 25,
                Company = new Company { Name = "Zebra" },
            },
            new User
            {
                Firstname = "C",
                Age = 20,
                Company = new Company { Name = "Alpha" },
            },
        }.AsQueryable();

        var query = new Query { OrderBy = "company/name asc, age desc" };
        var result = users.Apply(query);

        Assert.True(result.IsSuccess);
        var ordered = result.Value.Query.ToList();
        Assert.Equal("C", ordered[0].Firstname); // Alpha, 20
        Assert.Equal("A", ordered[1].Firstname); // Zebra, 30
        Assert.Equal("B", ordered[2].Firstname); // Zebra, 25
    }

    [Fact]
    public void Test_OrderBy_WithJsonNamingPolicy_SnakeCaseLower()
    {
        var users = new List<CamelCaseUser>
        {
            new CamelCaseUser { FirstName = "Charlie" },
            new CamelCaseUser { FirstName = "Alice" },
            new CamelCaseUser { FirstName = "Bob" },
        }.AsQueryable();

        var query = new Query { OrderBy = "first_name asc" };
        var options = new QueryOptions
        {
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.SnakeCaseLower,
        };
        var result = users.Apply(query, null, options);

        Assert.True(result.IsSuccess);
        var ordered = result.Value.Query.ToList();
        Assert.Equal("Alice", ordered[0].FirstName);
        Assert.Equal("Bob", ordered[1].FirstName);
        Assert.Equal("Charlie", ordered[2].FirstName);
    }
}
