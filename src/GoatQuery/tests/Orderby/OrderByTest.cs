using Xunit;

public sealed class OrderByTest
{
    public static IEnumerable<object[]> Parameters()
    {
        yield return new object[]
        {
            "age desc, firstname asc",
            new User[]
            {
                new User { Age = 3, Firstname = "Doe" },
                new User { Age = 3, Firstname = "Egg" },
                new User { Age = 2, Firstname = "Apple" },
                new User { Age = 2, Firstname = "John" },
                new User { Age = 1, Firstname = "Harry" },
                new User { Age = 1, Firstname = "Jane" },
            }
        };

        yield return new object[]
        {
            "age desc, firstname desc",
            new User[]
            {
                new User { Age = 3, Firstname = "Egg" },
                new User { Age = 3, Firstname = "Doe" },
                new User { Age = 2, Firstname = "John" },
                new User { Age = 2, Firstname = "Apple" },
                new User { Age = 1, Firstname = "Jane" },
                new User { Age = 1, Firstname = "Harry" },
            }
        };

        yield return new object[]
        {
            "age desc",
            new User[]
            {
                new User { Age = 3, Firstname = "Doe" },
                new User { Age = 3, Firstname = "Egg" },
                new User { Age = 2, Firstname = "John" },
                new User { Age = 2, Firstname = "Apple" },
                new User { Age = 1, Firstname = "Jane" },
                new User { Age = 1, Firstname = "Harry" },
            }
        };

        yield return new object[]
        {
            "Age asc",
            new User[]
            {
                new User { Age = 1, Firstname = "Jane" },
                new User { Age = 1, Firstname = "Harry" },
                new User { Age = 2, Firstname = "John" },
                new User { Age = 2, Firstname = "Apple" },
                new User { Age = 3, Firstname = "Doe" },
                new User { Age = 3, Firstname = "Egg" },
            }
        };

        yield return new object[]
        {
            "age",
            new User[]
            {
                new User { Age = 1, Firstname = "Jane" },
                new User { Age = 1, Firstname = "Harry" },
                new User { Age = 2, Firstname = "John" },
                new User { Age = 2, Firstname = "Apple" },
                new User { Age = 3, Firstname = "Doe" },
                new User { Age = 3, Firstname = "Egg" },
            }
        };

        yield return new object[]
        {
            "age asc",
            new User[]
            {
                new User { Age = 1, Firstname = "Jane" },
                new User { Age = 1, Firstname = "Harry" },
                new User { Age = 2, Firstname = "John" },
                new User { Age = 2, Firstname = "Apple" },
                new User { Age = 3, Firstname = "Doe" },
                new User { Age = 3, Firstname = "Egg" },
            }
        };

        yield return new object[]
        {
            "Age asc",
            new User[]
            {
                new User { Age = 1, Firstname = "Jane" },
                new User { Age = 1, Firstname = "Harry" },
                new User { Age = 2, Firstname = "John" },
                new User { Age = 2, Firstname = "Apple" },
                new User { Age = 3, Firstname = "Doe" },
                new User { Age = 3, Firstname = "Egg" },
            }
        };

        yield return new object[]
        {
            "aGe asc",
            new User[]
            {
                new User { Age = 1, Firstname = "Jane" },
                new User { Age = 1, Firstname = "Harry" },
                new User { Age = 2, Firstname = "John" },
                new User { Age = 2, Firstname = "Apple" },
                new User { Age = 3, Firstname = "Doe" },
                new User { Age = 3, Firstname = "Egg" },
            }
        };

        yield return new object[]
        {
            "AGe asc",
            new User[]
            {
                new User { Age = 1, Firstname = "Jane" },
                new User { Age = 1, Firstname = "Harry" },
                new User { Age = 2, Firstname = "John" },
                new User { Age = 2, Firstname = "Apple" },
                new User { Age = 3, Firstname = "Doe" },
                new User { Age = 3, Firstname = "Egg" },
            }
        };

        yield return new object[]
        {
            "aGE Asc",
            new User[]
            {
                new User { Age = 1, Firstname = "Jane" },
                new User { Age = 1, Firstname = "Harry" },
                new User { Age = 2, Firstname = "John" },
                new User { Age = 2, Firstname = "Apple" },
                new User { Age = 3, Firstname = "Doe" },
                new User { Age = 3, Firstname = "Egg" },
            }
        };

        yield return new object[]
        {
            "age aSc",
            new User[]
            {
                new User { Age = 1, Firstname = "Jane" },
                new User { Age = 1, Firstname = "Harry" },
                new User { Age = 2, Firstname = "John" },
                new User { Age = 2, Firstname = "Apple" },
                new User { Age = 3, Firstname = "Doe" },
                new User { Age = 3, Firstname = "Egg" },
            }
        };

        yield return new object[]
        {
            "",
            new User[]
            {
                new User { Age = 2, Firstname = "John" },
                new User { Age = 1, Firstname = "Jane" },
                new User { Age = 2, Firstname = "Apple" },
                new User { Age = 1, Firstname = "Harry" },
                new User { Age = 3, Firstname = "Doe" },
                new User { Age = 3, Firstname = "Egg" }
            }
        };
    }

    [Theory]
    [MemberData(nameof(Parameters))]
    public void Test_OrderBy(string orderby, IEnumerable<User> expected)
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
            OrderBy = orderby
        };

        var result = users.Apply(query);

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