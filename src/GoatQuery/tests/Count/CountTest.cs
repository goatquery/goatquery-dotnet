using Xunit;

public sealed class CountTest
{

    [Fact]
    public void Test_CountWithTrue()
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
            Count = true
        };

        var (queryable, count) = users.Apply(query);
        var results = queryable.ToArray();

        Assert.Equal(6, count);
        Assert.Equal(6, results.Count());
    }

    [Fact]
    public void Test_CountWithFalse()
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
            Count = false
        };

        var (_, count) = users.Apply(query);

        Assert.Null(count);
    }
}