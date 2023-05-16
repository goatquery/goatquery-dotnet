using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

public abstract class TestWithSqlite : IDisposable
{
    private const string InMemoryConnectionString = "DataSource=:memory:";
    private readonly SqliteConnection _connection;
    protected readonly TestDbContext _context;

    protected TestWithSqlite()
    {
        _connection = new SqliteConnection(InMemoryConnectionString);
        _connection.Open();
        var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseSqlite(_connection)
                .Options;
        _context = new TestDbContext(options);
        _context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _connection.Close();
    }
}