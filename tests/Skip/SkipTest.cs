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
                new User { Id = 1, Firstname = "Jane" },
                new User { Id = 2, Firstname = "John" },
                new User { Id = 2, Firstname = "Apple" },
                new User { Id = 3, Firstname = "Doe" },
                new User { Id = 3, Firstname = "Egg" }
            }
        };

        yield return new object[]
        {
            2,
            new User[]
            {
                new User { Id = 2, Firstname = "John" },
                new User { Id = 2, Firstname = "Apple" },
                new User { Id = 3, Firstname = "Doe" },
                new User { Id = 3, Firstname = "Egg" }
            }
        };

        yield return new object[]
        {
            3,
            new User[]
            {
                new User { Id = 2, Firstname = "Apple" },
                new User { Id = 3, Firstname = "Doe" },
                new User { Id = 3, Firstname = "Egg" }
            }
        };

        yield return new object[]
        {
            4,
            new User[]
            {
                new User { Id = 3, Firstname = "Doe" },
                new User { Id = 3, Firstname = "Egg" }
            }
        };

        yield return new object[]
        {
            5,
            new User[]
            {
                new User { Id = 3, Firstname = "Egg" }
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
            new User { Id = 1, Firstname = "Harry" },
            new User { Id = 1, Firstname = "Jane" },
            new User { Id = 2, Firstname = "John" },
            new User { Id = 2, Firstname = "Apple" },
            new User { Id = 3, Firstname = "Doe" },
            new User { Id = 3, Firstname = "Egg" }
        }.AsQueryable();

        var query = new Query
        {
            Skip = skip
        };

        var (queryable, _) = users.Apply(query);
        var results = queryable.ToArray();

        for (var i = 0; i < expected.Count(); i++)
        {
            var expectedUser = expected.ElementAt(i);
            var user = results.ElementAt(i);

            Assert.Equal(expectedUser.Id, user.Id);
            Assert.Equal(expectedUser.Firstname, user.Firstname);
        }
    }
}