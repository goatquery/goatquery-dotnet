using Xunit;

public sealed class SelectTest
{
    public static IEnumerable<object[]> Parameters()
    {
        yield return new object[]
        {
            "Id",
            new object[]
            {
                new { Id = 2 },
                new { Id = 1 },
            }
        };
    }

    [Theory]
    [MemberData(nameof(Parameters))]
    public void Test_Select(string select, IEnumerable<object> expected)
    {
        var users = new List<User>{
            new User { Id = 2, Firstname = "John" },
            new User { Id = 1, Firstname = "Jane" },
        }.AsQueryable();

        var query = new Query
        {
            Select = select
        };

        var (queryable, _) = users.Apply(query);
        var results = queryable.ToArray();

        Assert.Equal(expected, results);
    }
}