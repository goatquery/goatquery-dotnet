namespace GoatQuery.Tests;

using GoatQuery;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Testcontainers.PostgreSql;
using Xunit;

public class DatabaseTestFixture : IAsyncLifetime
{
    private PostgreSqlContainer? _postgresContainer;
    public TestDbContext DbContext { get; set; } = null!;

    public async Task InitializeAsync()
    {
        _postgresContainer = new PostgreSqlBuilder().WithImage("postgres:18-alpine").Build();

        await _postgresContainer.StartAsync();

        var connectionString = _postgresContainer.GetConnectionString();
        var optionsBuilder = new DbContextOptionsBuilder<TestDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        DbContext = new TestDbContext(optionsBuilder.Options);

        await DbContext.Database.EnsureCreatedAsync();

        await SeedTestData();
    }

    private async Task SeedTestData()
    {
        var users = TestData.Users.Values.ToList();

        await DbContext.Users.AddRangeAsync(users);
        await DbContext.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        if (DbContext != null)
        {
            await DbContext.DisposeAsync();
        }

        if (_postgresContainer != null)
        {
            await _postgresContainer.StopAsync();
            await _postgresContainer.DisposeAsync();
        }
    }
}
