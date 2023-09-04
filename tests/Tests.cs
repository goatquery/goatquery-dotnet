using System.Linq.Dynamic.Core.Exceptions;
using System.Linq.Expressions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace tests;

public class UserSearchTestBinder : ISearchBinder<User>
{
    public Expression<Func<User, bool>> Bind(string searchTerm)
    {
        Expression<Func<User, bool>> exp = x => EF.Functions.Like(x.Firstname, $"%{searchTerm}%") || EF.Functions.Like(x.Lastname, $"%{searchTerm}%");

        return exp;
    }
}

public class Tests : TestWithSqlite
{
    [Fact]
    public void Test_EmptyQuery()
    {
        var query = new Query();

        var (result, _) = _context.Users.AsQueryable().Apply(query);
        var sql = result.ToQueryString();

        var expectedSql = _context.Users.AsQueryable().ToQueryString();

        Assert.Equal(expectedSql, sql);
    }

    // Top

    [Fact]
    public void Test_QueryWithTop()
    {
        var query = new Query() { Top = 3 };

        var (result, _) = _context.Users.AsQueryable().Apply(query);
        var sql = result.ToQueryString();

        var expectedSql = _context.Users.AsQueryable().Take(query.Top).ToQueryString();

        Assert.Equal(expectedSql, sql);
    }

    [Fact]
    public void Test_QueryWithTopGreaterThanMaxTop()
    {
        var maxTop = 2;
        var query = new Query() { Top = 3 };

        GoatQueryException exception = Assert.Throws<GoatQueryException>(() => _context.Users.AsQueryable().Apply(query, maxTop));
    }

    // Skip

    [Fact]
    public void Test_QueryWithSkip()
    {
        var query = new Query() { Skip = 3 };

        var (result, _) = _context.Users.AsQueryable().Apply(query);
        var sql = result.ToQueryString();

        var expectedSql = _context.Users.AsQueryable().Skip(query.Skip).ToQueryString();

        Assert.Equal(expectedSql, sql);
    }

    // Count

    [Fact]
    public void Test_QueryWithCount()
    {
        var query = new Query() { Count = true };

        var (result, count) = _context.Users.AsQueryable().Apply(query);
        var sql = result.ToQueryString();

        var expectedSql = _context.Users.AsQueryable().ToQueryString();

        Assert.Equal(expectedSql, sql);
        Assert.NotNull(count);
    }

    // Order by

    [Fact]
    public void Test_QueryWithOrderby()
    {
        var query = new Query() { OrderBy = "firstname" };

        var (result, _) = _context.Users.AsQueryable().Apply(query);
        var sql = result.ToQueryString();

        var expectedSql = _context.Users.AsQueryable().OrderBy(x => x.Firstname).ToQueryString();

        Assert.Equal(expectedSql, sql);
    }

    [Fact]
    public void Test_QueryWithOrderbyAsc()
    {
        var query = new Query() { OrderBy = "firstname asc" };

        var (result, _) = _context.Users.AsQueryable().Apply(query);
        var sql = result.ToQueryString();

        var expectedSql = _context.Users.AsQueryable().OrderBy(x => x.Firstname).ToQueryString();

        Assert.Equal(expectedSql, sql);
    }

    [Fact]
    public void Test_QueryWithOrderbyDesc()
    {
        var query = new Query() { OrderBy = "firstname desc" };

        var (result, _) = _context.Users.AsQueryable().Apply(query);
        var sql = result.ToQueryString();

        var expectedSql = _context.Users.AsQueryable().OrderByDescending(x => x.Firstname).ToQueryString();

        Assert.Equal(expectedSql, sql);
    }

    [Fact]
    public void Test_QueryWithOrderbyMultiple()
    {
        var query = new Query() { OrderBy = "firstname asc, lastname desc" };

        var (result, _) = _context.Users.AsQueryable().Apply(query);
        var sql = result.ToQueryString();

        var expectedSql = _context.Users.AsQueryable().OrderBy(x => x.Firstname).ThenByDescending(x => x.Lastname).ToQueryString();

        Assert.Equal(expectedSql, sql);
    }

    // Select

    [Fact]
    public void Test_QueryWithSelect()
    {
        var query = new Query() { Select = "firstname, lastname" };

        var (result, _) = _context.Users.AsQueryable().Apply(query);
        var sql = result.ToQueryString();

        var expectedSql = _context.Users.AsQueryable().Select(x => new { x.Firstname, x.Lastname }).ToQueryString();

        Assert.Equal(expectedSql, sql);
    }

    [Fact]
    public void Test_QueryWithSelectInvalidColumn()
    {
        var query = new Query() { Select = "firstname, invalid-col" };

        ParseException exception = Assert.Throws<ParseException>(() => _context.Users.AsQueryable().Apply(query));
    }

    // Search

    [Fact]
    public void Test_QueryWithSearch()
    {
        var query = new Query() { Search = "Goat" };

        var (result, _) = _context.Users.AsQueryable().Apply(query, null, new UserSearchTestBinder());
        var sql = result.ToQueryString();

        var expectedSql = _context.Users.AsQueryable().Where(x =>
            EF.Functions.Like(x.Firstname, $"%{query.Search}%") ||
            EF.Functions.Like(x.Lastname, $"%{query.Search}%")
        ).ToQueryString();

        Assert.Equal(expectedSql, sql);
    }

    [Fact]
    public void Test_QueryWithSearchTermSpace()
    {
        var query = new Query() { Search = "Goat Query" };

        var (result, _) = _context.Users.AsQueryable().Apply(query, null, new UserSearchTestBinder());
        var sql = result.ToQueryString();

        var expectedSql = _context.Users.AsQueryable().Where(x =>
            EF.Functions.Like(x.Firstname, $"%{query.Search}%") ||
            EF.Functions.Like(x.Lastname, $"%{query.Search}%")
        ).ToQueryString();

        Assert.Equal(expectedSql, sql);
    }

    [Fact]
    public void Test_QueryWithSearchNilFunc()
    {
        var query = new Query() { Search = "Goat Query" };

        var (result, _) = _context.Users.AsQueryable().Apply(query, null);
        var sql = result.ToQueryString();

        var expectedSql = _context.Users.AsQueryable().ToQueryString();

        Assert.Equal(expectedSql, sql);
    }

    // Filter

    [Fact]
    public void Test_QueryWithFilterEquals()
    {
        var query = new Query() { Filter = "firstname eq 'goat'" };

        var (result, _) = _context.Users.AsQueryable().Apply(query, null);
        var sql = result.ToQueryString();

        var expectedSql = _context.Users.AsQueryable().Where(x => x.Firstname == "goat").ToQueryString();

        Assert.Equal(expectedSql, sql);
    }

    [Fact]
    public void Test_QueryWithFilterNotEquals()
    {
        var query = new Query() { Filter = "firstname ne 'goat'" };

        var (result, _) = _context.Users.AsQueryable().Apply(query, null);
        var sql = result.ToQueryString();

        var expectedSql = _context.Users.AsQueryable().Where(x => x.Firstname != "goat").ToQueryString();

        Assert.Equal(expectedSql, sql);
    }

    [Fact]
    public void Test_QueryWithFilterEqualsAndEquals()
    {
        var query = new Query() { Filter = "firstname eq 'goat' and lastname eq 'query'" };

        var (result, _) = _context.Users.AsQueryable().Apply(query, null);
        var sql = result.ToQueryString();

        var expectedSql = _context.Users.AsQueryable().Where(x => x.Firstname == "goat" && x.Lastname == "query").ToQueryString();

        Assert.Equal(expectedSql, sql);
    }

    [Fact]
    public void Test_QueryWithFilterEqualsAndNotEquals()
    {
        var query = new Query() { Filter = "firstname eq 'goat' and lastname ne 'query'" };

        var (result, _) = _context.Users.AsQueryable().Apply(query, null);
        var sql = result.ToQueryString();

        var expectedSql = _context.Users.AsQueryable().Where(x => x.Firstname == "goat" && x.Lastname != "query").ToQueryString();

        Assert.Equal(expectedSql, sql);
    }

    [Fact]
    public void Test_QueryWithFilterContains()
    {
        var query = new Query() { Filter = "firstname contains 'goat'" };

        var (result, _) = _context.Users.AsQueryable().Apply(query, null);
        var sql = result.ToQueryString();

        var expectedSql = _context.Users.AsQueryable()
            .Where(x => x.Firstname.Contains("goat"))
            .ToQueryString();

        Assert.Equal(expectedSql, sql);
    }

    [Fact]
    public void Test_QueryWithFilterContainsAndEquals()
    {
        var query = new Query() { Filter = "firstname contains 'goat' and lastname eq 'query'" };

        var (result, _) = _context.Users.AsQueryable().Apply(query, null);
        var sql = result.ToQueryString();

        var expectedSql = _context.Users.AsQueryable()
            .Where(x => x.Firstname.Contains("goat") && x.Lastname == "query")
            .ToQueryString();

        Assert.Equal(expectedSql, sql);
    }

    [Fact]
    public void Test_QueryWithFilterContainsOrEquals()
    {
        var query = new Query() { Filter = "firstname contains 'goat' or lastname eq 'query'" };

        var (result, _) = _context.Users.AsQueryable().Apply(query, null);
        var sql = result.ToQueryString();

        var expectedSql = _context.Users.AsQueryable()
            .Where(x => x.Firstname.Contains("goat") || x.Lastname == "query")
            .ToQueryString();

        Assert.Equal(expectedSql, sql);
    }

    [Fact]
    public void Test_QueryWithFilterEqualsWithConjunction()
    {
        var query = new Query() { Filter = "firstname eq 'goatand'" };

        var (result, _) = _context.Users.AsQueryable().Apply(query, null);
        var sql = result.ToQueryString();

        var expectedSql = _context.Users.AsQueryable()
            .Where(x => x.Firstname == "goatand")
            .ToQueryString();

        Assert.Equal(expectedSql, sql);
    }

    [Fact]
    public void Test_QueryWithFilterEqualsWithConjunctionAndSpaces()
    {
        var query = new Query() { Filter = "firstname eq ' and ' or lastname eq ' and or '" };

        var (result, _) = _context.Users.AsQueryable().Apply(query, null);
        var sql = result.ToQueryString();

        var expectedSql = _context.Users.AsQueryable()
            .Where(x => x.Firstname == " and " || x.Lastname == " and or ")
            .ToQueryString();

        Assert.Equal(expectedSql, sql);
    }

    [Fact]
    public void Test_QueryWithFilterCustomJsonPropertyName()
    {
        var query = new Query() { Filter = "displayName eq 'John'" };

        var (result, _) = _context.Users.AsQueryable().Apply(query, null);
        var sql = result.ToQueryString();

        var expectedSql = _context.Users.AsQueryable()
            .Where(x => x.UserName == "John")
            .ToQueryString();

        Assert.Equal(expectedSql, sql);
    }

    [Fact]
    public void Test_QueryWithFilterCustomColumnName()
    {
        var query = new Query() { Filter = "gender eq 'Male'" };

        var (result, _) = _context.Users.AsQueryable().Apply(query, null);
        var sql = result.ToQueryString();

        var expectedSql = _context.Users.AsQueryable()
            .Where(x => x.Gender == "Male")
            .ToQueryString();

        Assert.Equal(expectedSql, sql);
    }
}