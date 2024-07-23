using Xunit;

public sealed class TopTest
{
    [Theory]
    [InlineData(-1, 6)]
    [InlineData(0, 6)]
    [InlineData(1, 1)]
    [InlineData(2, 2)]
    [InlineData(3, 3)]
    [InlineData(4, 4)]
    [InlineData(5, 5)]
    [InlineData(100, 6)]
    [InlineData(100_000, 6)]
    public void Test_Top(int top, int expectedCount)
    {
        var users = new List<User>{
            new User { Age = 1, Firstname = "Jane" },
            new User { Age = 1, Firstname = "Harry" },
            new User { Age = 2, Firstname = "John" },
            new User { Age = 2, Firstname = "Apple" },
            new User { Age = 3, Firstname = "Doe" },
            new User { Age = 3, Firstname = "Egg" }
        }.AsQueryable();

        var query = new Query
        {
            Top = top
        };

        var result = users.Apply(query);

        Assert.Equal(expectedCount, result.Value.Query.Count());
    }

    [Theory]
    [InlineData(-1, 4)]
    [InlineData(0, 4)]
    [InlineData(1, 1)]
    [InlineData(2, 2)]
    [InlineData(3, 3)]
    [InlineData(4, 4)]
    public void Test_TopWithMaxTop(int top, int expectedCount)
    {
        var users = new List<User>{
            new User { Age = 1, Firstname = "Jane" },
            new User { Age = 1, Firstname = "Harry" },
            new User { Age = 2, Firstname = "John" },
            new User { Age = 2, Firstname = "Apple" },
            new User { Age = 3, Firstname = "Doe" },
            new User { Age = 3, Firstname = "Egg" }
        }.AsQueryable();

        var query = new Query
        {
            Top = top
        };

        var queryOptions = new QueryOptions
        {
            MaxTop = 4
        };

        var result = users.Apply(query, null, queryOptions);

        Assert.Equal(expectedCount, result.Value.Query.Count());
    }

    [Theory]
    [InlineData(5)]
    [InlineData(100)]
    [InlineData(100_000)]
    public void Test_TopWithMaxTopThrowsException(int top)
    {
        var users = new List<User>{
            new User { Age = 1, Firstname = "Jane" },
            new User { Age = 1, Firstname = "Harry" },
            new User { Age = 2, Firstname = "John" },
            new User { Age = 2, Firstname = "Apple" },
            new User { Age = 3, Firstname = "Doe" },
            new User { Age = 3, Firstname = "Egg" }
        }.AsQueryable();

        var query = new Query
        {
            Top = top
        };

        var queryOptions = new QueryOptions
        {
            MaxTop = 4
        };

        var result = users.Apply(query, null, queryOptions);

        Assert.True(result.IsFailed);
    }
}