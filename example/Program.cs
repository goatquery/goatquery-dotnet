using System.Reflection;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Bogus;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

Randomizer.Seed = new Random(8675309);

var builder = WebApplication.CreateBuilder(args);

var postgreSqlContainer = new PostgreSqlBuilder()
  .WithImage("postgres:15")
  .Build();

await postgreSqlContainer.StartAsync();

builder.Services.AddControllers();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(postgreSqlContainer.GetConnectionString());
});

builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await context.Database.EnsureCreatedAsync();

    // Seed data
    if (!context.Users.Any())
    {
        var companies = new Faker<Company>()
            .RuleFor(x => x.Name, f => f.Company.CompanyName())
            .RuleFor(x => x.Department, f => f.Commerce.Department());

        var cities = new Faker<City>()
            .RuleFor(x => x.Name, f => f.Address.City())
            .RuleFor(x => x.Country, f => f.Address.Country());

        var addresses = new Faker<Address>()
            .RuleFor(x => x.AddressLine1, f => f.Address.StreetAddress())
            .RuleFor(x => x.City, f => f.PickRandom(cities.Generate(50)));

        var users = new Faker<User>()
            .RuleFor(x => x.Firstname, f => f.Person.FirstName)
            .RuleFor(x => x.Lastname, f => f.Person.LastName)
            .RuleFor(x => x.Age, f => f.Random.Int(0, 100))
            .RuleFor(x => x.Gender, f => f.PickRandom<Gender>())
            .RuleFor(x => x.IsDeleted, f => f.Random.Bool())
            .RuleFor(x => x.Test, f => f.Random.Double())
            .RuleFor(x => x.NullableInt, f => f.Random.Bool() ? f.Random.Int(1, 100) : null)
            .RuleFor(x => x.IsEmailVerified, f => f.Random.Bool())
            .Rules((f, u) =>
            {
                var timeZone = TimeZoneInfo.FindSystemTimeZoneById("America/New_York");
                var date = f.Date.Past().ToUniversalTime();

                u.DateOfBirthUtc = date;
                u.DateOfBirthTz = TimeZoneInfo.ConvertTimeFromUtc(date, timeZone);
            })
            .RuleFor(x => x.Manager, (f, u) => f.CreateManager(3))
            .RuleFor(x => x.Addresses, f => f.PickRandom(addresses.Generate(5), f.Random.Int(1, 3)).ToList())
            .RuleFor(x => x.Tags, f => f.Lorem.Words(f.Random.Int(0, 5)).ToList())
            .RuleFor(x => x.Company, f => f.PickRandom(companies.Generate(20)));

        context.Users.AddRange(users.Generate(1_000));
        context.SaveChanges();

        Console.WriteLine("Seeded 1,000 fake users!");
    }
}

Console.WriteLine($"Postgres connection string: {postgreSqlContainer.GetConnectionString()}");

app.MapGet("/minimal/users", (ApplicationDbContext db, [FromServices] IMapper mapper, [AsParameters] Query query) =>
{
    var result = db.Users
        .Include(x => x.Company)
        .Include(x => x.Addresses)
            .ThenInclude(x => x.City)
        .Include(x => x.Manager)
            .ThenInclude(x => x.Manager)
        .Where(x => !x.IsDeleted)
        .ProjectTo<UserDto>(mapper.ConfigurationProvider)
        .Apply(query);

    if (result.IsFailed)
    {
        return Results.BadRequest(new { message = result.Errors });
    }

    var response = new PagedResponse<UserDto>(result.Value.Query.ToList(), result.Value.Count);

    return Results.Ok(response);
});

app.MapControllers();

app.Run();

public static class FakerExtensions
{
    public static User? CreateManager(this Faker f, int depth)
    {
        if (depth <= 0 || !f.Random.Bool(0.6f)) // 60% chance of having manager, stop at depth 0
            return null;

        return new User
        {
            Id = f.Random.Guid(),
            Firstname = f.Person.FirstName,
            Lastname = f.Person.LastName,
            Age = f.Random.Int(0, 100),
            IsDeleted = f.Random.Bool(),
            Test = f.Random.Double(),
            NullableInt = f.Random.Bool() ? f.Random.Int(1, 100) : null,
            IsEmailVerified = f.Random.Bool(),
            DateOfBirthUtc = f.Date.Past().ToUniversalTime(),
            DateOfBirthTz = TimeZoneInfo.ConvertTimeFromUtc(f.Date.Past().ToUniversalTime(), TimeZoneInfo.FindSystemTimeZoneById("America/New_York")),
            Manager = f.CreateManager(depth - 1) // Recursive call with reduced depth
        };
    }
}