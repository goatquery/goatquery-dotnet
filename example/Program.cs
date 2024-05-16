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
            .RuleFor(x => x.Email, f => f.Person.Email)
            .RuleFor(x => x.AvatarUrl, f => f.Internet.Avatar())
            .RuleFor(x => x.UserName, f => f.Person.UserName)
            .RuleFor(x => x.Gender, f => f.Person.Gender.ToString())
            .RuleFor(x => x.IsDeleted, f => f.Random.Bool());

        context.Users.AddRange(users.Generate(1_000));
        context.SaveChanges();

        Console.WriteLine("Seeded 1,000 fake users!");
    }
}

app.MapGet("/users", (ApplicationDbContext db, [FromServices] IMapper mapper, [AsParameters] Query query) =>
{
    var (users, _) = db.Users
        .Where(x => !x.IsDeleted)
        .ProjectTo<UserDto>(mapper.ConfigurationProvider)
        .Apply(query);

    return TypedResults.Ok(new PagedResponse<UserDto>(users.ToList(), 0));
});


app.Run();
