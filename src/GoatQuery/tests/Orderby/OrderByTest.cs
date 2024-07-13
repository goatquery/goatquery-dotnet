using Xunit;

public sealed record User
{
    public int Age { get; set; }
    public string Firstname { get; set; } = string.Empty;
}

public sealed class OrderByTest
{
    public static IEnumerable<object[]> Parameters()
    {
        yield return new object[]
        {
            "Age desc, firstname asc",
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
            "Age desc, firstname desc",
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
            "Age desc",
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
            "Age",
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
            "Age Asc",
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
            "Age aSc",
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

        var (queryable, _) = users.Apply(query);
        var results = queryable.ToArray();

        Assert.Equal(expected, results);
    }
}