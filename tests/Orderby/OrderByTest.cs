using Xunit;

public sealed record User
{
    public int Id { get; set; }
    public string Firstname { get; set; } = string.Empty;
}

public sealed class OrderByTest
{
    public static IEnumerable<object[]> Parameters()
    {
        yield return new object[]
        {
            "id desc, firstname asc",
            new User[]
            {
                new User { Id = 3, Firstname = "Doe" },
                new User { Id = 3, Firstname = "Egg" },
                new User { Id = 2, Firstname = "Apple" },
                new User { Id = 2, Firstname = "John" },
                new User { Id = 1, Firstname = "Harry" },
                new User { Id = 1, Firstname = "Jane" },
            }
        };

        yield return new object[]
        {
            "id desc, firstname desc",
            new User[]
            {
                new User { Id = 3, Firstname = "Egg" },
                new User { Id = 3, Firstname = "Doe" },
                new User { Id = 2, Firstname = "John" },
                new User { Id = 2, Firstname = "Apple" },
                new User { Id = 1, Firstname = "Jane" },
                new User { Id = 1, Firstname = "Harry" },
            }
        };

        yield return new object[]
        {
            "id desc",
            new User[]
            {
                new User { Id = 3, Firstname = "Doe" },
                new User { Id = 3, Firstname = "Egg" },
                new User { Id = 2, Firstname = "John" },
                new User { Id = 2, Firstname = "Apple" },
                new User { Id = 1, Firstname = "Jane" },
                new User { Id = 1, Firstname = "Harry" },
            }
        };

        yield return new object[]
        {
            "id asc",
            new User[]
            {
                new User { Id = 1, Firstname = "Jane" },
                new User { Id = 1, Firstname = "Harry" },
                new User { Id = 2, Firstname = "John" },
                new User { Id = 2, Firstname = "Apple" },
                new User { Id = 3, Firstname = "Doe" },
                new User { Id = 3, Firstname = "Egg" },
            }
        };

        yield return new object[]
        {
            "id",
            new User[]
            {
                new User { Id = 1, Firstname = "Jane" },
                new User { Id = 1, Firstname = "Harry" },
                new User { Id = 2, Firstname = "John" },
                new User { Id = 2, Firstname = "Apple" },
                new User { Id = 3, Firstname = "Doe" },
                new User { Id = 3, Firstname = "Egg" },
            }
        };

        yield return new object[]
        {
            "id asc",
            new User[]
            {
                new User { Id = 1, Firstname = "Jane" },
                new User { Id = 1, Firstname = "Harry" },
                new User { Id = 2, Firstname = "John" },
                new User { Id = 2, Firstname = "Apple" },
                new User { Id = 3, Firstname = "Doe" },
                new User { Id = 3, Firstname = "Egg" },
            }
        };

        yield return new object[]
        {
            "Id asc",
            new User[]
            {
                new User { Id = 1, Firstname = "Jane" },
                new User { Id = 1, Firstname = "Harry" },
                new User { Id = 2, Firstname = "John" },
                new User { Id = 2, Firstname = "Apple" },
                new User { Id = 3, Firstname = "Doe" },
                new User { Id = 3, Firstname = "Egg" },
            }
        };

        yield return new object[]
        {
            "ID asc",
            new User[]
            {
                new User { Id = 1, Firstname = "Jane" },
                new User { Id = 1, Firstname = "Harry" },
                new User { Id = 2, Firstname = "John" },
                new User { Id = 2, Firstname = "Apple" },
                new User { Id = 3, Firstname = "Doe" },
                new User { Id = 3, Firstname = "Egg" },
            }
        };

        yield return new object[]
        {
            "iD asc",
            new User[]
            {
                new User { Id = 1, Firstname = "Jane" },
                new User { Id = 1, Firstname = "Harry" },
                new User { Id = 2, Firstname = "John" },
                new User { Id = 2, Firstname = "Apple" },
                new User { Id = 3, Firstname = "Doe" },
                new User { Id = 3, Firstname = "Egg" },
            }
        };

        yield return new object[]
        {
            "id Asc",
            new User[]
            {
                new User { Id = 1, Firstname = "Jane" },
                new User { Id = 1, Firstname = "Harry" },
                new User { Id = 2, Firstname = "John" },
                new User { Id = 2, Firstname = "Apple" },
                new User { Id = 3, Firstname = "Doe" },
                new User { Id = 3, Firstname = "Egg" },
            }
        };

        yield return new object[]
        {
            "id aSc",
            new User[]
            {
                new User { Id = 1, Firstname = "Jane" },
                new User { Id = 1, Firstname = "Harry" },
                new User { Id = 2, Firstname = "John" },
                new User { Id = 2, Firstname = "Apple" },
                new User { Id = 3, Firstname = "Doe" },
                new User { Id = 3, Firstname = "Egg" },
            }
        };
    }

    [Theory]
    [MemberData(nameof(Parameters))]
    public void Test_OrderBy(string orderby, IEnumerable<User> expected)
    {
        var users = new List<User>{
            new User { Id = 2, Firstname = "John" },
            new User { Id = 1, Firstname = "Jane" },
            new User { Id = 2, Firstname = "Apple" },
            new User { Id = 1, Firstname = "Harry" },
            new User { Id = 3, Firstname = "Doe" },
            new User { Id = 3, Firstname = "Egg" }
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