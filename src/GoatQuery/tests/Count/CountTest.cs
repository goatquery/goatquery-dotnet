using Xunit;

public sealed class CountTest
{
    [Fact]
    public void Test_CountWithTrue()
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
            Count = true
        };

        var result = users.Apply(query);

        Assert.Equal(6, result.Value.Count);
        Assert.Equal(6, result.Value.Query.Count());
    }

    [Fact]
    public void Test_CountWithFalse()
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
            Count = false
        };

        var result = users.Apply(query);

        Assert.Null(result.Value.Count);
    }
}