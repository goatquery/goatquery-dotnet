using System.Reflection;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Bogus;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

Randomizer.Seed = new Random(123123123);

var builder = WebApplication.CreateBuilder(args);

var postgreSqlContainer = new PostgreSqlBuilder()
  .WithImage("postgres:15")
  .Build();

await postgreSqlContainer.StartAsync();

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
        var users = new Faker<User>()
            .RuleFor(x => x.Firstname, f => f.Person.FirstName)
            .RuleFor(x => x.Lastname, f => f.Person.LastName)
            .RuleFor(x => x.Age, f => f.Random.Int(0, 100))
            .RuleFor(x => x.IsDeleted, f => f.Random.Bool());

        context.Users.AddRange(users.Generate(1_000));
        context.SaveChanges();

        Console.WriteLine("Seeded 1,000 fake users!");
    }
}

app.MapGet("/users", (ApplicationDbContext db, [FromServices] IMapper mapper, [AsParameters] Query query) =>
{
    var (users, count) = db.Users
        .Where(x => !x.IsDeleted)
        .ProjectTo<UserDto>(mapper.ConfigurationProvider)
        .Apply(query);

    var response = new PagedResponse<UserDto>(users.ToList(), count);

    return TypedResults.Ok(response);
});


app.Run();
