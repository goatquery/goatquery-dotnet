using System.Reflection;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Bogus;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

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
        var users = new Faker<User>()
            .RuleFor(x => x.Firstname, f => f.Person.FirstName)
            .RuleFor(x => x.Lastname, f => f.Person.LastName)
            .RuleFor(x => x.Age, f => f.Random.Int(0, 100))
            .RuleFor(x => x.IsDeleted, f => f.Random.Bool())
            .RuleFor(x => x.Test, f => f.Random.Double());

        context.Users.AddRange(users.Generate(1_000));
        context.SaveChanges();

        Console.WriteLine("Seeded 1,000 fake users!");
    }
}

app.MapGet("/minimal/users", (ApplicationDbContext db, [FromServices] IMapper mapper, [AsParameters] Query query) =>
{
    var result = db.Users
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
