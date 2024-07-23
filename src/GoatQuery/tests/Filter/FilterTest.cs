using Xunit;

public sealed class FilterTest
{
    private static readonly Dictionary<string, User> _users = new Dictionary<string, User>
    {
        ["John"] = new User { Age = 2, Firstname = "John", UserId = Guid.Parse("58cdeca3-645b-457c-87aa-7d5f87734255") },
        ["Jane"] = new User { Age = 1, Firstname = "Jane", UserId = Guid.Parse("58cdeca3-645b-457c-87aa-7d5f87734255") },
        ["Apple"] = new User { Age = 2, Firstname = "Apple", UserId = Guid.Parse("58cdeca3-645b-457c-87aa-7d5f87734255") },
        ["Harry"] = new User { Age = 1, Firstname = "Harry", UserId = Guid.Parse("e4c7772b-8947-4e46-98ed-644b417d2a08") },
        ["Doe"] = new User { Age = 3, Firstname = "Doe", UserId = Guid.Parse("58cdeca3-645b-457c-87aa-7d5f87734255") },
        ["Egg"] = new User { Age = 3, Firstname = "Egg", UserId = Guid.Parse("58cdeca3-645b-457c-87aa-7d5f87734255") },
    };

    public static IEnumerable<object[]> Parameters()
    {
        yield return new object[] {
            "firstname eq 'John'",
            new[] { _users["John"] }
        };

        yield return new object[] {
            "firstname eq 'Random'",
            Array.Empty<User>()
        };

        yield return new object[] {
            "Age eq 1",
            new[] { _users["Jane"], _users["Harry"] }
        };

        yield return new object[] {
            "Age eq 0",
            Array.Empty<User>()
        };

        yield return new object[] {
            "firstname eq 'John' and Age eq 2",
            new[] { _users["John"] }
        };

        yield return new object[] {
            "firstname eq 'John' or Age eq 3",
            new[] { _users["John"], _users["Doe"], _users["Egg"] }
        };

        yield return new object[] {
            "Age eq 1 and firstName eq 'Harry' or Age eq 2",
            new[] { _users["John"], _users["Apple"], _users["Harry"] }
        };

        yield return new object[] {
            "Age eq 1 or Age eq 2 or firstName eq 'Egg'",
            new[] { _users["John"], _users["Jane"], _users["Apple"], _users["Harry"], _users["Egg"] }
        };

        yield return new object[] {
            "Age ne 3",
            new[] { _users["John"], _users["Jane"], _users["Apple"], _users["Harry"] }
        };

        yield return new object[] {
            "firstName contains 'a'",
            new[] { _users["Jane"], _users["Harry"] }
        };

        yield return new object[] {
            "Age ne 1 and firstName contains 'a'",
            Array.Empty<User>()
        };

        yield return new object[] {
            "Age ne 1 and firstName contains 'a' or firstName eq 'Apple'",
            new[] { _users["Apple"] }
        };

        yield return new object[] {
            "Firstname eq 'John' and Age eq 2 or Age eq 3",
            new[] { _users["John"], _users["Doe"], _users["Egg"] }
        };

        yield return new object[] {
            "(Firstname eq 'John' and Age eq 2) or Age eq 3",
            new[] { _users["John"], _users["Doe"], _users["Egg"] }
        };

        yield return new object[] {
            "Firstname eq 'John' and (Age eq 2 or Age eq 3)",
            new[] { _users["John"] }
        };

        yield return new object[] {
            "(Firstname eq 'John' and Age eq 2 or Age eq 3)",
            new[] { _users["John"], _users["Doe"], _users["Egg"] }
        };

        yield return new object[] {
            "(Firstname eq 'John') or (Age eq 3 and Firstname eq 'Egg') or Age eq 1 and (Age eq 2)",
            new[] { _users["John"], _users["Egg"] }
        };

        yield return new object[] {
            "UserId eq e4c7772b-8947-4e46-98ed-644b417d2a08",
            new[] { _users["Harry"] }
        };
    }

    [Theory]
    [MemberData(nameof(Parameters))]
    public void Test_Filter(string filter, IEnumerable<User> expected)
    {
        var query = new Query
        {
            Filter = filter
        };

        var result = _users.Values.AsQueryable().Apply(query);

        Assert.Equal(expected, result.Value.Query);
    }

    [Theory]
    [InlineData("NonExistentProperty eq 'John'")]
    public void Test_InvalidFilterThrowsException(string filter)
    {
        var query = new Query
        {
            Filter = filter
        };

        var result = _users.Values.AsQueryable().Apply(query);

        Assert.True(result.IsFailed);
    }

    [Fact]
    public void Test_Filter_WithCustomJsonPropertyName()
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
            Filter = "last_name eq 'John'"
        };

        var result = users.Apply(query);

        Assert.Equal(new List<CustomJsonPropertyUser>{
            new CustomJsonPropertyUser { Lastname = "John" },
        }, result.Value.Query);
    }
}