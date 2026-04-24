using Xunit;

public sealed class FilterTest : IClassFixture<DatabaseTestFixture>
{
    private readonly DatabaseTestFixture _fixture;

    public FilterTest(DatabaseTestFixture fixture)
    {
        _fixture = fixture;
    }

    public static IEnumerable<object[]> Parameters()
    {
        yield return new object[] { "firstname eq 'User01'", new[] { TestData.Users["User01"] } };

        yield return new object[] { "firstname eq 'Random'", Array.Empty<User>() };

        yield return new object[]
        {
            "Age eq 25",
            new[] { TestData.Users["User01"], TestData.Users["User05"] },
        };

        yield return new object[]
        {
            "Age eq 30",
            new[] { TestData.Users["User02"], TestData.Users["User03"] },
        };

        yield return new object[] { "Age eq 35", new[] { TestData.Users["User04"] } };

        yield return new object[] { "Age eq 0", Array.Empty<User>() };

        yield return new object[]
        {
            "firstname eq 'User02' and Age eq 30",
            new[] { TestData.Users["User02"] },
        };

        yield return new object[]
        {
            "firstname eq 'User01' or Age eq 35",
            new[] { TestData.Users["User01"], TestData.Users["User04"] },
        };

        yield return new object[]
        {
            "Age ne 30",
            new[] { TestData.Users["User01"], TestData.Users["User04"], TestData.Users["User05"] },
        };

        yield return new object[]
        {
            "firstName contains '0'",
            new[]
            {
                TestData.Users["User01"],
                TestData.Users["User02"],
                TestData.Users["User03"],
                TestData.Users["User04"],
                TestData.Users["User05"],
            },
        };

        yield return new object[] { "firstName contains '5'", new[] { TestData.Users["User05"] } };

        yield return new object[]
        {
            "Firstname eq 'User01' and Age eq 25 or Age eq 30",
            new[] { TestData.Users["User01"], TestData.Users["User02"], TestData.Users["User03"] },
        };

        yield return new object[]
        {
            "(Firstname eq 'User01' and Age eq 25) or Age eq 30",
            new[] { TestData.Users["User01"], TestData.Users["User02"], TestData.Users["User03"] },
        };

        yield return new object[]
        {
            "Firstname eq 'User01' and (Age eq 25 or Age eq 30)",
            new[] { TestData.Users["User01"] },
        };

        yield return new object[]
        {
            "(Firstname eq 'User02' and Age eq 30 or Age eq 35)",
            new[] { TestData.Users["User02"], TestData.Users["User04"] },
        };

        yield return new object[]
        {
            "(Firstname eq 'User01') or (Age eq 35 and Firstname eq 'User04') or Age eq 25 and (Age eq 30)",
            new[] { TestData.Users["User01"], TestData.Users["User04"] },
        };

        yield return new object[]
        {
            "id eq 11111111-1111-1111-1111-111111111111",
            new[] { TestData.Users["User01"] },
        };

        yield return new object[]
        {
            "age lt 30",
            new[] { TestData.Users["User01"], TestData.Users["User05"] },
        };

        yield return new object[]
        {
            "age lte 30",
            new[]
            {
                TestData.Users["User01"],
                TestData.Users["User02"],
                TestData.Users["User03"],
                TestData.Users["User05"],
            },
        };

        yield return new object[]
        {
            "age gt 25",
            new[] { TestData.Users["User02"], TestData.Users["User03"], TestData.Users["User04"] },
        };

        yield return new object[]
        {
            "age gte 30",
            new[] { TestData.Users["User02"], TestData.Users["User03"], TestData.Users["User04"] },
        };

        yield return new object[]
        {
            "age lt 35 and age gt 25",
            new[] { TestData.Users["User02"], TestData.Users["User03"] },
        };

        yield return new object[]
        {
            "balanceDecimal eq 1500.75m",
            new[] { TestData.Users["User01"] },
        };

        yield return new object[]
        {
            "balanceDecimal eq 500.00m",
            new[] { TestData.Users["User02"] },
        };

        yield return new object[]
        {
            "balanceDecimal gt 100m",
            new[] { TestData.Users["User01"], TestData.Users["User02"] },
        };

        yield return new object[]
        {
            "balanceDecimal eq null",
            new[] { TestData.Users["User03"], TestData.Users["User04"] },
        };

        yield return new object[]
        {
            "balanceDecimal ne null",
            new[] { TestData.Users["User01"], TestData.Users["User02"], TestData.Users["User05"] },
        };

        yield return new object[]
        {
            "balanceDouble eq 2500.50d",
            new[] { TestData.Users["User01"] },
        };

        yield return new object[]
        {
            "balanceDouble eq null",
            new[] { TestData.Users["User02"], TestData.Users["User04"] },
        };

        yield return new object[]
        {
            "balanceFloat eq 3500.25f",
            new[] { TestData.Users["User01"] },
        };

        yield return new object[] { "balanceFloat eq 750.50f", new[] { TestData.Users["User02"] } };

        yield return new object[]
        {
            "balanceFloat eq null",
            new[] { TestData.Users["User03"], TestData.Users["User04"], TestData.Users["User05"] },
        };

        yield return new object[]
        {
            "dateOfBirth eq 1998-03-15T10:30:00Z",
            new[] { TestData.Users["User01"] },
        };

        yield return new object[]
        {
            "dateOfBirth eq 1993-07-20T14:00:00Z",
            new[] { TestData.Users["User02"] },
        };

        yield return new object[]
        {
            "dateOfBirth lt 1995-01-01T00:00:00Z",
            new[] { TestData.Users["User02"], TestData.Users["User03"] },
        };

        yield return new object[]
        {
            "dateOfBirth gte 1993-01-01T00:00:00Z",
            new[]
            {
                TestData.Users["User01"],
                TestData.Users["User02"],
                TestData.Users["User03"],
                TestData.Users["User05"],
            },
        };

        yield return new object[] { "dateOfBirth eq null", new[] { TestData.Users["User04"] } };

        yield return new object[]
        {
            "dateOfBirth ne null",
            new[]
            {
                TestData.Users["User01"],
                TestData.Users["User02"],
                TestData.Users["User03"],
                TestData.Users["User05"],
            },
        };

        yield return new object[]
        {
            "age gt 25 and isEmailVerified eq true",
            new[] { TestData.Users["User03"] },
        };

        yield return new object[]
        {
            "balanceDecimal ne null and age lt 30",
            new[] { TestData.Users["User01"], TestData.Users["User05"] },
        };

        yield return new object[]
        {
            "manager ne null and isEmailVerified eq false",
            new[] { TestData.Users["User02"], TestData.Users["User04"] },
        };

        yield return new object[]
        {
            "(age eq 25 or age eq 30) and company ne null",
            new[]
            {
                TestData.Users["User01"],
                TestData.Users["User02"],
                TestData.Users["User03"],
                TestData.Users["User05"],
            },
        };

        yield return new object[]
        {
            "isEmailVerified eq true",
            new[] { TestData.Users["User01"], TestData.Users["User03"], TestData.Users["User05"] },
        };

        yield return new object[]
        {
            "isEmailVerified eq false",
            new[] { TestData.Users["User02"], TestData.Users["User04"] },
        };

        yield return new object[]
        {
            "isEmailVerified ne true",
            new[] { TestData.Users["User02"], TestData.Users["User04"] },
        };

        yield return new object[]
        {
            "isEmailVerified ne false",
            new[] { TestData.Users["User01"], TestData.Users["User03"], TestData.Users["User05"] },
        };

        yield return new object[]
        {
            "manager/firstName eq 'User01'",
            new[] { TestData.Users["User02"] },
        };

        yield return new object[]
        {
            "manager/firstName eq 'User02'",
            new[] { TestData.Users["User03"], TestData.Users["User04"] },
        };

        yield return new object[]
        {
            "manager/age gt 28",
            new[] { TestData.Users["User03"], TestData.Users["User04"] },
        };

        yield return new object[]
        {
            "manager/isEmailVerified eq true",
            new[] { TestData.Users["User02"] },
        };

        yield return new object[]
        {
            "manager/manager/firstName eq 'User01'",
            new[] { TestData.Users["User03"], TestData.Users["User04"] },
        };

        yield return new object[]
        {
            "manager/manager eq null",
            new[] { TestData.Users["User01"], TestData.Users["User02"], TestData.Users["User05"] },
        };

        yield return new object[]
        {
            "company/name eq 'TechCorp'",
            new[] { TestData.Users["User01"] },
        };

        yield return new object[]
        {
            "company/name eq 'DataSoft'",
            new[] { TestData.Users["User02"] },
        };

        yield return new object[]
        {
            "company/name contains 'Corp'",
            new[] { TestData.Users["User01"], TestData.Users["User05"] },
        };

        yield return new object[]
        {
            "addresses/any(addr: addr/city/name eq 'New York')",
            new[] { TestData.Users["User01"] },
        };

        yield return new object[]
        {
            "addresses/any(address: address/city/name eq 'Chicago')",
            new[] { TestData.Users["User01"] },
        };

        yield return new object[]
        {
            "addresses/any(a: a/city/name eq 'Seattle')",
            new[] { TestData.Users["User02"] },
        };

        yield return new object[]
        {
            "addresses/any(addr: addr/city/name eq 'Miami')",
            new[] { TestData.Users["User05"] },
        };

        yield return new object[]
        {
            "addresses/any(addr: addr/city/name eq 'NonExistentCity')",
            Array.Empty<User>(),
        };

        yield return new object[]
        {
            "addresses/all(addr: addr/city/country eq 'USA')",
            new[] { TestData.Users["User01"], TestData.Users["User02"], TestData.Users["User05"] },
        };

        yield return new object[]
        {
            "addresses/any(addr: addr/addressLine1 contains 'Main')",
            new[] { TestData.Users["User01"] },
        };

        yield return new object[]
        {
            "addresses/any(addr: addr/addressLine1 contains 'Oak')",
            new[] { TestData.Users["User01"] },
        };

        yield return new object[]
        {
            "addresses/any(addr: addr/city/name eq 'New York' or addr/city/name eq 'Seattle')",
            new[] { TestData.Users["User01"], TestData.Users["User02"] },
        };

        yield return new object[]
        {
            "firstname eq 'User01' and addresses/any(addr: addr/city/name eq 'New York')",
            new[] { TestData.Users["User01"] },
        };

        yield return new object[] { "tags/any(x: x eq 'vip')", new[] { TestData.Users["User01"] } };

        yield return new object[]
        {
            "tags/any(x: x eq 'premium')",
            new[] { TestData.Users["User01"], TestData.Users["User02"] },
        };

        yield return new object[]
        {
            "tags/any(x: x eq 'standard')",
            new[] { TestData.Users["User05"] },
        };

        yield return new object[]
        {
            "status eq 0",
            new[] { TestData.Users["User01"], TestData.Users["User02"], TestData.Users["User05"] },
        };

        yield return new object[]
        {
            "status eq 1",
            new[] { TestData.Users["User03"], TestData.Users["User04"] },
        };

        yield return new object[]
        {
            "status ne 0",
            new[] { TestData.Users["User03"], TestData.Users["User04"] },
        };

        yield return new object[]
        {
            "status ne 1",
            new[] { TestData.Users["User01"], TestData.Users["User02"], TestData.Users["User05"] },
        };

        yield return new object[]
        {
            "gender eq 'Male'",
            new[] { TestData.Users["User01"], TestData.Users["User05"] },
        };

        yield return new object[]
        {
            "gender eq 'male'",
            new[] { TestData.Users["User01"], TestData.Users["User05"] },
        };

        yield return new object[] { "gender eq 'Female'", new[] { TestData.Users["User02"] } };

        yield return new object[] { "gender eq 'Alternative'", new[] { TestData.Users["User03"] } };

        yield return new object[]
        {
            "gender ne 'Male'",
            new[] { TestData.Users["User02"], TestData.Users["User03"], TestData.Users["User04"] },
        };

        yield return new object[]
        {
            "gender ne 'Female'",
            new[]
            {
                TestData.Users["User01"],
                TestData.Users["User03"],
                TestData.Users["User04"],
                TestData.Users["User05"],
            },
        };

        yield return new object[] { "gender eq null", new[] { TestData.Users["User04"] } };

        yield return new object[]
        {
            "gender ne null",
            new[]
            {
                TestData.Users["User01"],
                TestData.Users["User02"],
                TestData.Users["User03"],
                TestData.Users["User05"],
            },
        };

        yield return new object[]
        {
            "gender eq 'Male' and age eq 25",
            new[] { TestData.Users["User01"], TestData.Users["User05"] },
        };

        yield return new object[]
        {
            "status eq 0 and age gt 25",
            new[] { TestData.Users["User02"] },
        };

        yield return new object[]
        {
            "gender eq 'Female' or status eq 1",
            new[] { TestData.Users["User02"], TestData.Users["User03"], TestData.Users["User04"] },
        };

        yield return new object[]
        {
            "gender eq 'Male' or gender eq 'Female'",
            new[] { TestData.Users["User01"], TestData.Users["User02"], TestData.Users["User05"] },
        };

        yield return new object[]
        {
            "(gender eq 'Male' and status eq 0) or age eq 30",
            new[]
            {
                TestData.Users["User01"],
                TestData.Users["User02"],
                TestData.Users["User03"],
                TestData.Users["User05"],
            },
        };

        yield return new object[]
        {
            "gender ne null and status eq 1",
            new[] { TestData.Users["User03"] },
        };

        yield return new object[] { "firstname eq 'user01'", new[] { TestData.Users["User01"] } };
    }

    [Theory]
    [MemberData(nameof(Parameters))]
    public void Test_Filter(string filter, IEnumerable<User> expected)
    {
        var query = new Query
        {
            Filter = filter,
            OrderBy = "Firstname", // Ensure consistent ordering for tests
        };

        var result = _fixture.DbContext.Users.Apply(query);

        Assert.Equal(expected, result.Value.Query);
    }

    [Theory]
    [InlineData("NonExistentProperty eq 'John'")]
    [InlineData("manager//firstName eq 'John'")]
    [InlineData("manager/ eq 'John'")]
    [InlineData("/manager eq 'John'")]
    [InlineData("addresses/any(addr: addr/nonExistentProperty eq 'test')")]
    [InlineData("addresses/invalid(addr: addr/city/name eq 'test')")]
    [InlineData("nonExistentCollection/any(item: item eq 'test')")]
    public void Test_InvalidFilterReturnsError(string filter)
    {
        var query = new Query { Filter = filter };

        var result = _fixture.DbContext.Users.Apply(query);

        Assert.True(result.IsFailed);
    }

    [Fact]
    public void Test_Filter_WithCustomJsonPropertyName()
    {
        var users = new List<CustomJsonPropertyUser>
        {
            new CustomJsonPropertyUser { Lastname = "John" },
            new CustomJsonPropertyUser { Lastname = "Jane" },
            new CustomJsonPropertyUser { Lastname = "Apple" },
            new CustomJsonPropertyUser { Lastname = "Harry" },
            new CustomJsonPropertyUser { Lastname = "Doe" },
            new CustomJsonPropertyUser { Lastname = "Egg" },
        }.AsQueryable();

        var query = new Query { Filter = "last_name eq 'John'" };

        var result = users.Apply(query);

        Assert.Equal(
            new List<CustomJsonPropertyUser> { new CustomJsonPropertyUser { Lastname = "John" } },
            result.Value.Query
        );
    }

    [Fact]
    public void Test_Filter_WithJsonNamingPolicy_SnakeCaseLower()
    {
        var users = new List<CamelCaseUser>
        {
            new CamelCaseUser { FirstName = "John", Age = 25 },
            new CamelCaseUser { FirstName = "Jane", Age = 30 },
        }.AsQueryable();

        var query = new Query { Filter = "first_name eq 'John'" };
        var options = new QueryOptions
        {
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.SnakeCaseLower,
        };
        var result = users.Apply(query, null, options);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Query);
        Assert.Equal("John", result.Value.Query.First().FirstName);
    }

    [Fact]
    public void Test_Filter_WithJsonNamingPolicy_NestedProperty()
    {
        var users = new List<CamelCaseUser>
        {
            new CamelCaseUser
            {
                FirstName = "John",
                Company = new CamelCaseCompany { CompanyName = "TechCorp" },
            },
            new CamelCaseUser
            {
                FirstName = "Jane",
                Company = new CamelCaseCompany { CompanyName = "DataSoft" },
            },
        }.AsQueryable();

        var query = new Query { Filter = "company/company_name eq 'TechCorp'" };
        var options = new QueryOptions
        {
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.SnakeCaseLower,
        };
        var result = users.Apply(query, null, options);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Query);
        Assert.Equal("John", result.Value.Query.First().FirstName);
    }

    [Fact]
    public void Test_Filter_StringEq_WithNullValues_DoesNotThrow()
    {
        var users = new List<NullableStringUser>
        {
            new NullableStringUser { Firstname = "John", Age = 1 },
            new NullableStringUser { Firstname = null, Age = 2 },
            new NullableStringUser { Firstname = "Jane", Age = 3 },
        }.AsQueryable();

        var query = new Query { Filter = "firstname eq 'John'" };
        var result = users.Apply(query);

        Assert.True(result.IsSuccess);
        var results = result.Value.Query.ToList();
        Assert.Single(results);
        Assert.Equal("John", results.First().Firstname);
    }

    [Fact]
    public void Test_Filter_StringNe_WithNullValues_IncludesNulls()
    {
        var users = new List<NullableStringUser>
        {
            new NullableStringUser { Firstname = "John", Age = 1 },
            new NullableStringUser { Firstname = null, Age = 2 },
            new NullableStringUser { Firstname = "Jane", Age = 3 },
        }.AsQueryable();

        var query = new Query { Filter = "firstname ne 'John'" };
        var result = users.Apply(query);

        Assert.True(result.IsSuccess);
        var results = result.Value.Query.ToList();
        Assert.Equal(2, results.Count);
        Assert.Contains(results, u => u.Firstname == null);
        Assert.Contains(results, u => u.Firstname == "Jane");
    }

    [Fact]
    public void Test_Filter_StringContains_WithNullValues_DoesNotThrow()
    {
        var users = new List<NullableStringUser>
        {
            new NullableStringUser { Firstname = "John", Age = 1 },
            new NullableStringUser { Firstname = null, Age = 2 },
            new NullableStringUser { Firstname = "Jane", Age = 3 },
        }.AsQueryable();

        var query = new Query { Filter = "firstname contains 'oh'" };
        var result = users.Apply(query);

        Assert.True(result.IsSuccess);
        var results = result.Value.Query.ToList();
        Assert.Single(results);
        Assert.Equal("John", results.First().Firstname);
    }

    [Fact]
    public void Test_Filter_LambdaRespectsMaxPropertyMappingDepth()
    {
        var data = new List<DepthTestNode>
        {
            new DepthTestNode
            {
                Name = "A",
                Child = new DepthTestChild
                {
                    Value = "X",
                    Items = new[]
                    {
                        new DepthTestGrandchild
                        {
                            Label = "item1",
                            Deep = new DepthTestGreatGrandchild { Detail = "deep_detail" },
                        },
                    },
                },
            },
        }.AsQueryable();

        var query = new Query { Filter = "child/items/any(i: i/deep/detail eq 'deep_detail')" };
        var options = new QueryOptions { MaxPropertyMappingDepth = 1 };
        var result = data.Apply(query, null, options);

        Assert.True(result.IsFailed);
    }

    [Fact]
    public void Test_Filter_DateTimeOffset_Eq()
    {
        var timestamp = DateTimeOffset.Parse("2024-01-15T10:30:00+05:00");
        var users = new List<DateTimeOffsetUser>
        {
            new DateTimeOffsetUser { Name = "A", CreatedAt = timestamp },
            new DateTimeOffsetUser
            {
                Name = "B",
                CreatedAt = DateTimeOffset.Parse("2024-06-01T00:00:00Z"),
            },
        }.AsQueryable();

        var query = new Query { Filter = "createdAt eq 2024-01-15T10:30:00+05:00" };
        var result = users.Apply(query);

        Assert.True(result.IsSuccess);
        var results = result.Value.Query.ToList();
        Assert.Single(results);
        Assert.Equal("A", results.First().Name);
    }

    [Fact]
    public void Test_Filter_DateTimeOffset_Comparison()
    {
        var users = new List<DateTimeOffsetUser>
        {
            new DateTimeOffsetUser
            {
                Name = "A",
                CreatedAt = DateTimeOffset.Parse("2024-01-01T00:00:00Z"),
            },
            new DateTimeOffsetUser
            {
                Name = "B",
                CreatedAt = DateTimeOffset.Parse("2024-06-01T00:00:00Z"),
            },
            new DateTimeOffsetUser
            {
                Name = "C",
                CreatedAt = DateTimeOffset.Parse("2024-12-01T00:00:00Z"),
            },
        }.AsQueryable();

        var query = new Query { Filter = "createdAt lt 2024-07-01T00:00:00Z" };
        var result = users.Apply(query);

        Assert.True(result.IsSuccess);
        var results = result.Value.Query.ToList();
        Assert.Equal(2, results.Count);
    }

    [Fact]
    public void Test_Filter_NullableDateTimeOffset_EqNull()
    {
        var users = new List<DateTimeOffsetUser>
        {
            new DateTimeOffsetUser
            {
                Name = "A",
                UpdatedAt = DateTimeOffset.Parse("2024-01-01T00:00:00Z"),
            },
            new DateTimeOffsetUser { Name = "B", UpdatedAt = null },
        }.AsQueryable();

        var query = new Query { Filter = "updatedAt eq null" };
        var result = users.Apply(query);

        Assert.True(result.IsSuccess);
        var results = result.Value.Query.ToList();
        Assert.Single(results);
        Assert.Equal("B", results.First().Name);
    }

    [Fact]
    public void Test_Filter_NullableDateTimeOffset_NeNull()
    {
        var users = new List<DateTimeOffsetUser>
        {
            new DateTimeOffsetUser
            {
                Name = "A",
                UpdatedAt = DateTimeOffset.Parse("2024-01-01T00:00:00Z"),
            },
            new DateTimeOffsetUser { Name = "B", UpdatedAt = null },
        }.AsQueryable();

        var query = new Query { Filter = "updatedAt ne null" };
        var result = users.Apply(query);

        Assert.True(result.IsSuccess);
        var results = result.Value.Query.ToList();
        Assert.Single(results);
        Assert.Equal("A", results.First().Name);
    }

    public record IntegerConverts
    {
        public long Long { get; set; }
        public short Short { get; set; }
        public byte Byte { get; set; }
        public uint Uint { get; set; }
        public ulong ULong { get; set; }
        public ushort UShort { get; set; }
        public sbyte SByte { get; set; }
    }

    [Theory]
    [InlineData("long eq 10")]
    [InlineData("short eq 20")]
    [InlineData("byte eq 30")]
    [InlineData("uint eq 40")]
    [InlineData("ulong eq 50")]
    [InlineData("ushort eq 60")]
    [InlineData("sbyte eq 70")]
    public void Test_Filter_CanConvertIntToOtherNumericTypes(string filter)
    {
        var users = new List<IntegerConverts>
        {
            new IntegerConverts()
            {
                Long = 0,
                Short = 0,
                Byte = 0,
                Uint = 0,
                ULong = 0,
                UShort = 0,
                SByte = 0,
            },
            new IntegerConverts()
            {
                Long = 10,
                Short = 20,
                Byte = 30,
                Uint = 40,
                ULong = 50,
                UShort = 60,
                SByte = 70,
            },
            new IntegerConverts()
            {
                Long = 1,
                Short = 2,
                Byte = 3,
                Uint = 4,
                ULong = 5,
                UShort = 6,
                SByte = 7,
            },
        }.AsQueryable();

        var query = new Query { Filter = filter };

        var result = users.Apply(query);

        Assert.Equal(
            new List<IntegerConverts>
            {
                new IntegerConverts()
                {
                    Long = 10,
                    Short = 20,
                    Byte = 30,
                    Uint = 40,
                    ULong = 50,
                    UShort = 60,
                    SByte = 70,
                },
            },
            result.Value.Query
        );
    }
}
