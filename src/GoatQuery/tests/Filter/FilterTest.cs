using Xunit;

public sealed class FilterTest
{
    public static IEnumerable<object[]> Parameters()
    {
        yield return new object[]
        {
            "firstname eq 'John'",
            new User[]
            {
                new User { Age = 2, Firstname = "John" },
            }
        };

        yield return new object[]
        {
            "firstname eq 'Random'",
            new User[] {}
        };

        yield return new object[]
        {
            "Age eq 1",
            new User[] {
                new User { Age = 1, Firstname = "Jane" },
                new User { Age = 1, Firstname = "Harry" },
            }
        };

        yield return new object[]
        {
            "Age eq 0",
            new User[] {}
        };

        yield return new object[]
        {
            "firstname eq 'John' and Age eq 2",
            new User[]
            {
                new User { Age = 2, Firstname = "John" },
            }
        };

        yield return new object[]
        {
            "firstname eq 'John' or Age eq 3",
            new User[]
            {
                new User { Age = 2, Firstname = "John" },
                new User { Age = 3, Firstname = "Doe" },
                new User { Age = 3, Firstname = "Egg" }
            }
        };

        yield return new object[]
        {
            "Age eq 1 and firstName eq 'Harry' or Age eq 2",
            new User[]
            {
                new User { Age = 2, Firstname = "John" },
                new User { Age = 2, Firstname = "Apple" },
                new User { Age = 1, Firstname = "Harry" },
            }
        };

        yield return new object[]
        {
            "Age eq 1 or Age eq 2 or firstName eq 'Egg'",
            new User[]
            {
                new User { Age = 2, Firstname = "John" },
                new User { Age = 1, Firstname = "Jane" },
                new User { Age = 2, Firstname = "Apple" },
                new User { Age = 1, Firstname = "Harry" },
                new User { Age = 3, Firstname = "Egg" }
            }
        };

        yield return new object[]
        {
            "Age ne 3",
            new User[]
            {
                new User { Age = 2, Firstname = "John" },
                new User { Age = 1, Firstname = "Jane" },
                new User { Age = 2, Firstname = "Apple" },
                new User { Age = 1, Firstname = "Harry" },
            }
        };

        yield return new object[]
        {
            "firstName contains 'a'",
            new User[]
            {
                new User { Age = 1, Firstname = "Jane" },
                new User { Age = 1, Firstname = "Harry" },
            }
        };

        yield return new object[]
        {
            "Age ne 1 and firstName contains 'a'",
            new User[] {}
        };

        yield return new object[]
        {
            "Age ne 1 and firstName contains 'a' or firstName eq 'Apple'",
            new User[]
            {
                new User { Age = 2, Firstname = "Apple" },
            }
        };

        yield return new object[]
        {
            "Firstname eq 'John' and Age eq 2 or Age eq 3",
            new User[]
            {
                new User { Age = 2, Firstname = "John" },
                new User { Age = 3, Firstname = "Doe" },
                new User { Age = 3, Firstname = "Egg" }
            }
        };

        yield return new object[]
        {
            "(Firstname eq 'John' and Age eq 2) or Age eq 3",
            new User[]
            {
                new User { Age = 2, Firstname = "John" },
                new User { Age = 3, Firstname = "Doe" },
                new User { Age = 3, Firstname = "Egg" }
            }
        };

        yield return new object[]
        {
            "Firstname eq 'John' and (Age eq 2 or Age eq 3)",
            new User[]
            {
                new User { Age = 2, Firstname = "John" },
            }
        };

        yield return new object[]
        {
            "(Firstname eq 'John' and Age eq 2 or Age eq 3)",
            new User[]
            {
                new User { Age = 2, Firstname = "John" },
                new User { Age = 3, Firstname = "Doe" },
                new User { Age = 3, Firstname = "Egg" }
            }
        };

        yield return new object[]
        {
            "(Firstname eq 'John') or (Age eq 3 and Firstname eq 'Egg') or Age eq 1 and (Age eq 2)",
            new User[]
            {
                new User { Age = 2, Firstname = "John" },
                new User { Age = 3, Firstname = "Egg" }
            }
        };
    }

    [Theory]
    [MemberData(nameof(Parameters))]
    public void Test_Filter(string filter, IEnumerable<User> expected)
    {
        var users = new List<User>{
            new User { Age = 2, Firstname = "John" },
            new User { Age = 1, Firstname = "Jane" },
            new User { Age = 2, Firstname = "Apple" },
            new User { Age = 1, Firstname = "Harry" },
            new User { Age = 3, Firstname = "Doe" },
            new User { Age = 3, Firstname = "Egg" }
        }.AsQueryable();

        var query = new Query
        {
            Filter = filter
        };

        var result = users.Apply(query);

        Assert.Equal(expected, result.Value.Query);
    }

    [Theory]
    [InlineData("NonExistentProperty eq 'John'")]
    public void Test_InvalidFilterThrowsException(string filter)
    {
        var users = new List<User>{
            new User { Age = 2, Firstname = "John" },
            new User { Age = 1, Firstname = "Jane" },
            new User { Age = 2, Firstname = "Apple" },
            new User { Age = 1, Firstname = "Harry" },
            new User { Age = 3, Firstname = "Doe" },
            new User { Age = 3, Firstname = "Egg" }
        }.AsQueryable();

        var query = new Query
        {
            Filter = filter
        };

        var result = users.Apply(query);

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