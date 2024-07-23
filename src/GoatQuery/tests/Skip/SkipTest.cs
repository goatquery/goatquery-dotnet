using Xunit;

public sealed class SkipTest
{
    public static IEnumerable<object[]> Parameters()
    {
        yield return new object[]
        {
            1,
            new User[]
            {
                new User { Age = 1, Firstname = "Jane" },
                new User { Age = 2, Firstname = "John" },
                new User { Age = 2, Firstname = "Apple" },
                new User { Age = 3, Firstname = "Doe" },
                new User { Age = 3, Firstname = "Egg" }
            }
        };

        yield return new object[]
        {
            2,
            new User[]
            {
                new User { Age = 2, Firstname = "John" },
                new User { Age = 2, Firstname = "Apple" },
                new User { Age = 3, Firstname = "Doe" },
                new User { Age = 3, Firstname = "Egg" }
            }
        };

        yield return new object[]
        {
            3,
            new User[]
            {
                new User { Age = 2, Firstname = "Apple" },
                new User { Age = 3, Firstname = "Doe" },
                new User { Age = 3, Firstname = "Egg" }
            }
        };

        yield return new object[]
        {
            4,
            new User[]
            {
                new User { Age = 3, Firstname = "Doe" },
                new User { Age = 3, Firstname = "Egg" }
            }
        };

        yield return new object[]
        {
            5,
            new User[]
            {
                new User { Age = 3, Firstname = "Egg" }
            }
        };

        yield return new object[]
        {
            6,
            new User[] { }
        };

        yield return new object[]
        {
            7,
            new User[] { }
        };

        yield return new object[]
        {
            10_000,
            new User[] { }
        };
    }

    [Theory]
    [MemberData(nameof(Parameters))]
    public void Test_Skip(int skip, IEnumerable<User> expected)
    {
        var users = new List<User>{
            new User { Age = 1, Firstname = "Harry" },
            new User { Age = 1, Firstname = "Jane" },
            new User { Age = 2, Firstname = "John" },
            new User { Age = 2, Firstname = "Apple" },
            new User { Age = 3, Firstname = "Doe" },
            new User { Age = 3, Firstname = "Egg" }
        }.AsQueryable();

        var query = new Query
        {
            Skip = skip
        };

        var result = users.Apply(query);

        Assert.Equal(expected, result.Value.Query);
    }
}