namespace GoatQuery.Tests;

using GoatQuery;
using Xunit;

public sealed class FilterStressTest : IClassFixture<DatabaseTestFixture>
{
    private readonly DatabaseTestFixture _fixture;

    public FilterStressTest(DatabaseTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void NestedProperty_Lambda_LogicalOperators()
    {
        // Users whose company name contains "Tech" AND have at least one address in "USA"
        // TechCorp (User01, has addresses in USA) and Tech Solutions (User03, no addresses)
        // Result: User01 only
        var query = new Query
        {
            Filter = "company/name contains 'Tech' and addresses/any(a: a/city/country eq 'USA')",
        };
        var result = _fixture.DbContext.Set<User>().Apply(query);

        Assert.True(result.IsSuccess);
        var results = result.Value.Query.ToList();
        Assert.Equal(1, results.Count);
        Assert.Equal("User01", results[0].Firstname);
    }

    [Fact]
    public void DateRange_NullHandling_OrGrouping()
    {
        // Users born in 1998 (full datetime range) OR with null DOB
        // User01 (1998-03-15), User05 (1998-12-25), User04 (null DOB)
        var query = new Query
        {
            Filter =
                "(dateOfBirth gte 1998-01-01T00:00:00Z and dateOfBirth lt 1999-01-01T00:00:00Z) or dateOfBirth eq null",
        };
        var result = _fixture.DbContext.Set<User>().Apply(query);

        Assert.True(result.IsSuccess);
        var results = result.Value.Query.ToList();
        Assert.Equal(3, results.Count);
        var names = results.Select(u => u.Firstname).OrderBy(n => n).ToList();
        Assert.Equal(new[] { "User01", "User04", "User05" }, names);
    }

    [Fact]
    public void NegativeNumber_NestedProperty_OrderBy()
    {
        // Users with balanceDecimal > -1 (excludes nulls), ordered by company/name asc
        // User01=1500.75, User02=500.00, User05=0.00 (all > -1); User03/User04 are null
        var query = new Query { Filter = "balanceDecimal gt -1", OrderBy = "company/name asc" };
        var result = _fixture.DbContext.Set<User>().Apply(query);

        Assert.True(result.IsSuccess);
        var results = result.Value.Query.ToList();
        Assert.Equal(3, results.Count);
        // Order by company/name: DataSoft, TechCorp, WebCorp
        Assert.Equal("User02", results[0].Firstname);
        Assert.Equal("User01", results[1].Firstname);
        Assert.Equal("User05", results[2].Firstname);
    }

    [Fact]
    public void Lambda_OrInBody_CombinedWithRootFilter()
    {
        // Active users who have an address in "New York" or "Miami"
        // User01 (Active, New York+Chicago), User05 (Active, Miami), User02 (Active, Seattle — no match)
        var query = new Query
        {
            Filter =
                "status eq 'Active' and addresses/any(a: a/city/name eq 'New York' or a/city/name eq 'Miami')",
        };
        var result = _fixture.DbContext.Set<User>().Apply(query);

        Assert.True(result.IsSuccess);
        var results = result.Value.Query.ToList();
        Assert.Equal(2, results.Count);
        var names = results.Select(u => u.Firstname).OrderBy(n => n).ToList();
        Assert.Equal(new[] { "User01", "User05" }, names);
    }

    [Fact]
    public void PrimitiveCollectionLambda_Boolean_NestedProperty()
    {
        // Users who are email verified AND have "premium" tag AND work in Engineering
        // User01: verified=true, tags=["vip","premium"], dept=Engineering — match
        // User02: verified=false — no
        // User03: verified=true, tags=[] — no
        // User05: verified=true, tags=["standard"] — no
        var query = new Query
        {
            Filter =
                "isEmailVerified eq true and tags/any(t: t eq 'premium') and company/department eq 'Engineering'",
        };
        var result = _fixture.DbContext.Set<User>().Apply(query);

        Assert.True(result.IsSuccess);
        var results = result.Value.Query.ToList();
        Assert.Equal(1, results.Count);
        Assert.Equal("User01", results[0].Firstname);
    }
}
