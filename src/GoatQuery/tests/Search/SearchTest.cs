using System.Linq.Expressions;
using Xunit;

public class UserSearchTestBinder : ISearchBinder<User>
{
    public Expression<Func<User, bool>> Bind(string searchTerm)
    {
        var term = searchTerm.ToLower();

        Expression<Func<User, bool>> exp = x =>
            x.Firstname.ToLower().Contains(term);

        return exp;
    }
}

public sealed class SearchTest
{
    [Theory]
    [InlineData("john", 1)]
    [InlineData("JOHN", 1)]
    [InlineData("j", 2)]
    [InlineData("e", 4)]
    [InlineData("eg", 1)]
    public void Test_Search(string searchTerm, int expectedCount)
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
            Search = searchTerm
        };

        var (queryable, _) = users.Apply(query, new UserSearchTestBinder());
        var results = queryable.ToArray();

        Assert.Equal(expectedCount, results.Count());
    }
}