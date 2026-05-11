namespace GoatQuery.Tests;

using GoatQuery;
using Xunit;

public sealed class FilterTypeMismatchTest
{
    public record NumericEntity
    {
        public Guid Id { get; set; }
        public decimal Price { get; set; }
        public float Score { get; set; }
        public double Rating { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int Quantity { get; set; }
        public DateTime? CreatedDate { get; set; }
    }

    private static IQueryable<NumericEntity> TestData =>
        new List<NumericEntity>
        {
            new NumericEntity
            {
                Id = Guid.NewGuid(),
                Price = 10m,
                Score = 5.0f,
                Rating = 8.5,
                Name = "Test",
                IsActive = true,
                Quantity = 5,
                CreatedDate = DateTime.UtcNow,
            },
        }.AsQueryable();

    [Theory]
    [InlineData("price gt 3.30")]
    [InlineData("price gt 100")]
    [InlineData("score gt 3.30")]
    [InlineData("score gt 100")]
    [InlineData("rating gt 3.30")]
    [InlineData("rating gt 100")]
    public void Test_Filter_NumericPromotion_Succeeds(string filter)
    {
        var query = new Query { Filter = filter };
        var result = TestData.Apply(query);

        Assert.True(result.IsSuccess, string.Join("; ", result.Errors.Select(e => e.Message)));
    }

    [Theory]
    [InlineData("quantity gt 99999999999999999999999999999999")]
    public void Test_Filter_NumericOverflow_ReturnsError(string filter)
    {
        var query = new Query { Filter = filter };
        var result = TestData.Apply(query);

        Assert.True(result.IsFailed);
    }

    // When a fractional number is used against an integer property, the evaluator
    // promotes both sides to double instead of erroring. No integer can exactly
    // equal a fractional number, so eq returns empty and ne returns all.
    // Ordering operators compare mathematically (e.g., 5 gt 3.30 → true).
    [Theory]
    [InlineData("quantity eq 3.30", 0)]
    [InlineData("quantity ne 3.30", 1)]
    [InlineData("quantity gt 3.30", 1)]
    [InlineData("quantity gte 3.30", 1)]
    [InlineData("quantity lt 3.30", 0)]
    [InlineData("quantity lte 3.30", 0)]
    [InlineData("quantity eq 5.0", 1)]
    [InlineData("quantity gt 5.0", 0)]
    [InlineData("quantity lt 5.0", 0)]
    public void Test_Filter_DoubleOnIntegerProperty_PromotesToDouble(
        string filter,
        int expectedCount
    )
    {
        var query = new Query { Filter = filter };
        var result = TestData.Apply(query);

        Assert.True(result.IsSuccess, string.Join("; ", result.Errors.Select(e => e.Message)));
        Assert.Equal(expectedCount, result.Value.Query.Count());
    }

    [Theory]
    [InlineData("id eq null", 0)]
    [InlineData("price eq null", 0)]
    [InlineData("quantity eq null", 0)]
    [InlineData("isActive eq null", 0)]
    [InlineData("id ne null", 1)]
    [InlineData("price ne null", 1)]
    [InlineData("isActive ne null", 1)]
    public void Test_Filter_NullOnNonNullable_ReturnsCorrectCount(string filter, int expectedCount)
    {
        var query = new Query { Filter = filter };
        var result = TestData.Apply(query);

        Assert.True(result.IsSuccess, string.Join("; ", result.Errors.Select(e => e.Message)));
        Assert.Equal(expectedCount, result.Value.Query.Count());
    }

    [Theory]
    [InlineData("name eq null")]
    [InlineData("createdDate eq null")]
    public void Test_Filter_NullOnNullable_Succeeds(string filter)
    {
        var query = new Query { Filter = filter };
        var result = TestData.Apply(query);

        Assert.True(result.IsSuccess, string.Join("; ", result.Errors.Select(e => e.Message)));
    }

    [Theory]
    [InlineData("name gt 'abc'")]
    [InlineData("name lt 'abc'")]
    [InlineData("name gte 'abc'")]
    [InlineData("name lte 'abc'")]
    [InlineData("id gt '00000000-0000-0000-0000-000000000000'")]
    [InlineData("isActive gt true")]
    public void Test_Filter_OrderingOnIncompatibleType_ReturnsError(string filter)
    {
        var query = new Query { Filter = filter };
        var result = TestData.Apply(query);

        Assert.True(result.IsFailed);
    }

    [Theory]
    [InlineData("name eq 'abc'")]
    [InlineData("name ne 'abc'")]
    [InlineData("name contains 'abc'")]
    [InlineData("id eq '00000000-0000-0000-0000-000000000000'")]
    [InlineData("id eq 00000000-0000-0000-0000-000000000000")]
    public void Test_Filter_EqualityOnStringAndGuid_Succeeds(string filter)
    {
        var query = new Query { Filter = filter };
        var result = TestData.Apply(query);

        Assert.True(result.IsSuccess, string.Join("; ", result.Errors.Select(e => e.Message)));
    }

    [Theory]
    [InlineData("price eq 'hello'")]
    [InlineData("quantity eq 'hello'")]
    [InlineData("isActive eq 'hello'")]
    public void Test_Filter_StringLiteralOnNonStringProperty_ReturnsError(string filter)
    {
        var query = new Query { Filter = filter };
        var result = TestData.Apply(query);

        Assert.True(result.IsFailed);
    }

    [Theory]
    [InlineData("price contains '5'")]
    [InlineData("quantity contains '5'")]
    [InlineData("isActive contains 'true'")]
    public void Test_Filter_ContainsOnNonString_ReturnsError(string filter)
    {
        var query = new Query { Filter = filter };
        var result = TestData.Apply(query);

        Assert.True(result.IsFailed);
    }

    [Theory]
    [InlineData("quantity gt -1", 1)]
    [InlineData("quantity lt -1", 0)]
    [InlineData("quantity eq -5", 0)]
    [InlineData("quantity ne -5", 1)]
    [InlineData("quantity gt -0.5", 1)]
    [InlineData("quantity eq -0.5", 0)]
    [InlineData("quantity ne -0.5", 1)]
    [InlineData("quantity lt -0.5", 0)]
    [InlineData("price gt -10.5", 1)]
    [InlineData("rating gt -1.5", 1)]
    public void Test_Filter_NegativeNumbers_ReturnsCorrectCount(string filter, int expectedCount)
    {
        var query = new Query { Filter = filter };
        var result = TestData.Apply(query);

        Assert.True(result.IsSuccess, string.Join("; ", result.Errors.Select(e => e.Message)));
        Assert.Equal(expectedCount, result.Value.Query.Count());
    }
}
