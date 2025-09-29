public static class TestData
{
    private static readonly Guid User01Id = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid User02Id = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid User03Id = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid User04Id = Guid.Parse("44444444-4444-4444-4444-444444444444");
    private static readonly Guid User05Id = Guid.Parse("55555555-5555-5555-5555-555555555555");

    public static readonly Dictionary<string, User> Users = new Dictionary<string, User>
    {
        ["User01"] = new User
        {
            Id = User01Id,
            Age = 25,
            Firstname = "User01",
            DateOfBirth = DateTime.SpecifyKind(DateTime.Parse("1998-03-15 10:30:00"), DateTimeKind.Utc),
            BalanceDecimal = 1500.75m,
            BalanceDouble = 2500.50d,
            BalanceFloat = 3500.25f,
            IsEmailVerified = true,
            ManagerId = null,
            Company = new Company
            {
                Id = Guid.NewGuid(),
                Name = "TechCorp",
                Department = "Engineering"
            },
            Addresses =
            [
                new Address
                {
                    Id = Guid.NewGuid(),
                    AddressLine1 = "123 Main St",
                    City = new City
                    {
                        Id = Guid.NewGuid(),
                        Name = "New York",
                        Country = "USA"
                    }
                },
                new Address
                {
                    Id = Guid.NewGuid(),
                    AddressLine1 = "456 Oak Ave",
                    City = new City
                    {
                        Id = Guid.NewGuid(),
                        Name = "Chicago",
                        Country = "USA"
                    }
                }
            ],
            Tags = ["vip", "premium"]
        },
        ["User02"] = new User
        {
            Id = User02Id,
            Age = 30,
            Firstname = "User02",
            DateOfBirth = DateTime.SpecifyKind(DateTime.Parse("1993-07-20 14:00:00"), DateTimeKind.Utc),
            BalanceDecimal = 500.00m,
            BalanceDouble = null,
            BalanceFloat = 750.50f,
            IsEmailVerified = false,
            ManagerId = User01Id,
            Company = new Company
            {
                Id = Guid.NewGuid(),
                Name = "DataSoft",
                Department = "Development"
            },
            Addresses =
            [
                new Address
                {
                    Id = Guid.NewGuid(),
                    AddressLine1 = "789 Pine St",
                    City = new City
                    {
                        Id = Guid.NewGuid(),
                        Name = "Seattle",
                        Country = "USA"
                    }
                }
            ],
            Tags = ["premium"]
        },
        ["User03"] = new User
        {
            Id = User03Id,
            Age = 30,
            Firstname = "User03",
            DateOfBirth = DateTime.SpecifyKind(DateTime.Parse("1993-11-10 09:15:00"), DateTimeKind.Utc),
            BalanceDecimal = null,
            BalanceDouble = 1000.00d,
            BalanceFloat = null,
            IsEmailVerified = true,
            ManagerId = User02Id,
            Company = new Company
            {
                Id = Guid.NewGuid(),
                Name = "Tech Solutions",
                Department = "Sales"
            },
            Addresses = Array.Empty<Address>(),
            Tags = Array.Empty<string>()
        },
        ["User04"] = new User
        {
            Id = User04Id,
            Age = 35,
            Firstname = "User04",
            DateOfBirth = null,
            BalanceDecimal = null,
            BalanceDouble = null,
            BalanceFloat = null,
            IsEmailVerified = false,
            ManagerId = User02Id,
            Company = null,
            Addresses = Array.Empty<Address>(),
            Tags = Array.Empty<string>()
        },
        ["User05"] = new User
        {
            Id = User05Id,
            Age = 25,
            Firstname = "User05",
            DateOfBirth = DateTime.SpecifyKind(DateTime.Parse("1998-12-25 18:45:00"), DateTimeKind.Utc),
            BalanceDecimal = 0.00m,
            BalanceDouble = 0.00d,
            BalanceFloat = null,
            IsEmailVerified = true,
            ManagerId = null,
            Company = new Company
            {
                Id = Guid.NewGuid(),
                Name = "WebCorp",
                Department = "Marketing"
            },
            Addresses =
            [
                new Address
                {
                    Id = Guid.NewGuid(),
                    AddressLine1 = "999 Broadway",
                    City = new City
                    {
                        Id = Guid.NewGuid(),
                        Name = "Miami",
                        Country = "USA"
                    }
                }
            ],
            Tags = ["standard"]
        }
    };
}