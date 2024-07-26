using Xunit;

public sealed class FilterTest
{
    private static readonly Dictionary<string, User> _users = new Dictionary<string, User>
    {
        ["John"] = new User { Age = 2, Firstname = "John", UserId = Guid.Parse("58cdeca3-645b-457c-87aa-7d5f87734255"), DateOfBirth = DateTime.Parse("2004-01-31 23:59:59") },
        ["Jane"] = new User { Age = 1, Firstname = "Jane", UserId = Guid.Parse("58cdeca3-645b-457c-87aa-7d5f87734255"), DateOfBirth = DateTime.Parse("2020-05-09 15:30:00") },
        ["Apple"] = new User { Age = 2, Firstname = "Apple", UserId = Guid.Parse("58cdeca3-645b-457c-87aa-7d5f87734255"), DateOfBirth = DateTime.Parse("1980-12-31 00:00:01") },
        ["Harry"] = new User { Age = 1, Firstname = "Harry", UserId = Guid.Parse("e4c7772b-8947-4e46-98ed-644b417d2a08"), DateOfBirth = DateTime.Parse("2002-08-01") },
        ["Doe"] = new User { Age = 3, Firstname = "Doe", UserId = Guid.Parse("58cdeca3-645b-457c-87aa-7d5f87734255"), DateOfBirth = DateTime.Parse("2023-07-26 12:00:30") },
        ["Egg"] = new User { Age = 3, Firstname = "Egg", UserId = Guid.Parse("58cdeca3-645b-457c-87aa-7d5f87734255"), DateOfBirth = DateTime.Parse("2000-01-01 00:00:00") },
    };

    public static IEnumerable<object[]> Parameters()
    {
        yield return new object[] {
            "firstname eq 'John'",
            new[] { _users["John"] }
        };

        yield return new object[] {
            "firstname eq 'Random'",
            Array.Empty<User>()
        };

        yield return new object[] {
            "Age eq 1",
            new[] { _users["Jane"], _users["Harry"] }
        };

        yield return new object[] {
            "Age eq 0",
            Array.Empty<User>()
        };

        yield return new object[] {
            "firstname eq 'John' and Age eq 2",
            new[] { _users["John"] }
        };

        yield return new object[] {
            "firstname eq 'John' or Age eq 3",
            new[] { _users["John"], _users["Doe"], _users["Egg"] }
        };

        yield return new object[] {
            "Age eq 1 and firstName eq 'Harry' or Age eq 2",
            new[] { _users["John"], _users["Apple"], _users["Harry"] }
        };

        yield return new object[] {
            "Age eq 1 or Age eq 2 or firstName eq 'Egg'",
            new[] { _users["John"], _users["Jane"], _users["Apple"], _users["Harry"], _users["Egg"] }
        };

        yield return new object[] {
            "Age ne 3",
            new[] { _users["John"], _users["Jane"], _users["Apple"], _users["Harry"] }
        };

        yield return new object[] {
            "firstName contains 'a'",
            new[] { _users["Jane"], _users["Harry"] }
        };

        yield return new object[] {
            "Age ne 1 and firstName contains 'a'",
            Array.Empty<User>()
        };

        yield return new object[] {
            "Age ne 1 and firstName contains 'a' or firstName eq 'Apple'",
            new[] { _users["Apple"] }
        };

        yield return new object[] {
            "Firstname eq 'John' and Age eq 2 or Age eq 3",
            new[] { _users["John"], _users["Doe"], _users["Egg"] }
        };

        yield return new object[] {
            "(Firstname eq 'John' and Age eq 2) or Age eq 3",
            new[] { _users["John"], _users["Doe"], _users["Egg"] }
        };

        yield return new object[] {
            "Firstname eq 'John' and (Age eq 2 or Age eq 3)",
            new[] { _users["John"] }
        };

        yield return new object[] {
            "(Firstname eq 'John' and Age eq 2 or Age eq 3)",
            new[] { _users["John"], _users["Doe"], _users["Egg"] }
        };

        yield return new object[] {
            "(Firstname eq 'John') or (Age eq 3 and Firstname eq 'Egg') or Age eq 1 and (Age eq 2)",
            new[] { _users["John"], _users["Egg"] }
        };

        yield return new object[] {
            "UserId eq e4c7772b-8947-4e46-98ed-644b417d2a08",
            new[] { _users["Harry"] }
        };

        yield return new object[] {
            "age lt 3",
            new[] { _users["John"], _users["Jane"], _users["Apple"], _users["Harry"] }
        };

        yield return new object[] {
            "age lt 1",
            Array.Empty<User>()
        };

        yield return new object[] {
            "age lte 2",
            new[] { _users["John"], _users["Jane"], _users["Apple"], _users["Harry"] }
        };

        yield return new object[] {
            "age gt 1",
            new[] { _users["John"], _users["Apple"], _users["Doe"], _users["Egg"] }
        };

        yield return new object[] {
            "age gte 3",
            new[] { _users["Doe"], _users["Egg"] }
        };

        yield return new object[] {
            "age lt 3 and age gt 1",
            new[] { _users["John"], _users["Apple"] }
        };

        yield return new object[] {
            "dateOfBirth eq 2000-01-01",
            new[] { _users["Egg"] }
        };

        yield return new object[] {
            "dateOfBirth lt 2010-01-01",
            new[] { _users["John"], _users["Apple"], _users["Harry"], _users["Egg"] }
        };

        yield return new object[] {
            "dateOfBirth lte 2002-08-01",
            new[] { _users["Apple"], _users["Harry"], _users["Egg"] }
        };

        yield return new object[] {
            "dateOfBirth gt 2000-08-01 and dateOfBirth lt 2023-01-01",
            new[] { _users["John"], _users["Jane"], _users["Harry"] }
        };

        yield return new object[] {
            "dateOfBirth eq 2023-07-26T12:00:30Z",
            new[] { _users["Doe"] }
        };

        yield return new object[] {
            "dateOfBirth gte 2000-01-01",
            new[] { _users["John"], _users["Jane"], _users["Harry"], _users["Doe"], _users["Egg"] }
        };

        yield return new object[] {
            "dateOfBirth gte 2000-01-01 and dateOfBirth lte 2020-05-09T15:29:59",
            new[] { _users["John"], _users["Harry"], _users["Egg"] }
        };
    }

    [Theory]
    [MemberData(nameof(Parameters))]
    public void Test_Filter(string filter, IEnumerable<User> expected)
    {
        var query = new Query
        {
            Filter = filter
        };

        var result = _users.Values.AsQueryable().Apply(query);

        Assert.Equal(expected, result.Value.Query);
    }

    [Theory]
    [InlineData("NonExistentProperty eq 'John'")]
    public void Test_InvalidFilterThrowsException(string filter)
    {
        var query = new Query
        {
            Filter = filter
        };

        var result = _users.Values.AsQueryable().Apply(query);

        Assert.True(result.IsFailed);
    }

    [Fact]
    public void Test_Filter_WithCustomJsonPropertyName()
    {
        var users = new List<CustomJsonPropertyUser>{
            new CustomJsonPropertyUser { Lastname = "John" },
            new CustomJsonPropertyUser { Lastname = "Jane" },
            new CustomJsonPropertyUser { Lastname = "Apple" },
            new CustomJsonPropertyUser { Lastname = "Harry" },
            new CustomJsonPropertyUser { Lastname = "Doe" },
            new CustomJsonPropertyUser { Lastname = "Egg" }
        }.AsQueryable();

        var query = new Query
        {
            Filter = "last_name eq 'John'"
        };

        var result = users.Apply(query);

        Assert.Equal(new List<CustomJsonPropertyUser>{
            new CustomJsonPropertyUser { Lastname = "John" },
        }, result.Value.Query);
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
        var users = new List<IntegerConverts>{
            new IntegerConverts() { Long = 0, Short = 0, Byte = 0, Uint = 0, ULong = 0, UShort = 0, SByte = 0},
            new IntegerConverts() { Long = 10, Short = 20, Byte = 30, Uint = 40, ULong = 50, UShort = 60, SByte = 70},
            new IntegerConverts() { Long = 1, Short = 2, Byte = 3, Uint = 4, ULong = 5, UShort = 6, SByte = 7},
        }.AsQueryable();

        var query = new Query
        {
            Filter = filter
        };

        var result = users.Apply(query);

        Assert.Equal(new List<IntegerConverts>{
            new IntegerConverts() { Long = 10, Short = 20, Byte = 30, Uint = 40, ULong = 50, UShort = 60, SByte = 70},
        }, result.Value.Query);
    }
}