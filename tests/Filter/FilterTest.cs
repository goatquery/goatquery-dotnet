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
                new User { Id = 2, Firstname = "John" },
            }
        };

        yield return new object[]
        {
            "firstname eq 'Random'",
            new User[] {}
        };

        yield return new object[]
        {
            "id eq 1",
            new User[] {
                new User { Id = 1, Firstname = "Jane" },
                new User { Id = 1, Firstname = "Harry" },
            }
        };

        yield return new object[]
        {
            "id eq 0",
            new User[] {}
        };

        yield return new object[]
        {
            "firstname eq 'John' and id eq 2",
            new User[]
            {
                new User { Id = 2, Firstname = "John" },
            }
        };

        yield return new object[]
        {
            "firstname eq 'John' or id eq 3",
            new User[]
            {
                new User { Id = 2, Firstname = "John" },
                new User { Id = 3, Firstname = "Doe" },
                new User { Id = 3, Firstname = "Egg" }
            }
        };

        yield return new object[]
        {
            "id eq 1 and firstName eq 'Harry' or id eq 2",
            new User[]
            {
                new User { Id = 2, Firstname = "John" },
                new User { Id = 2, Firstname = "Apple" },
                new User { Id = 1, Firstname = "Harry" },
            }
        };

        yield return new object[]
        {
            "id eq 1 or id eq 2 or firstName eq 'Egg'",
            new User[]
            {
                new User { Id = 2, Firstname = "John" },
                new User { Id = 1, Firstname = "Jane" },
                new User { Id = 2, Firstname = "Apple" },
                new User { Id = 1, Firstname = "Harry" },
                new User { Id = 3, Firstname = "Egg" }
            }
        };

        yield return new object[]
        {
            "id ne 3",
            new User[]
            {
                new User { Id = 2, Firstname = "John" },
                new User { Id = 1, Firstname = "Jane" },
                new User { Id = 2, Firstname = "Apple" },
                new User { Id = 1, Firstname = "Harry" },
            }
        };
    }

    [Theory]
    [MemberData(nameof(Parameters))]
    public void Test_Filter(string filter, IEnumerable<User> expected)
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
            Filter = filter
        };

        var (queryable, _) = users.Apply(query);
        var results = queryable.ToArray();

        Assert.Equal(expected, results);
    }
}