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
            new User[] {}
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

        for (var i = 0; i < expected.Count(); i++)
        {
            var expectedUser = expected.ElementAt(i);
            var user = results.ElementAt(i);

            Assert.Equal(expectedUser.Id, user.Id);
            Assert.Equal(expectedUser.Firstname, user.Firstname);
        }
    }
}