using System.Linq.Dynamic.Core;
using System.Reflection;
using Bogus;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.OperationFilter<GoatQueryOpenAPIFilter>();
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseInMemoryDatabase("minimal");
});

builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddSingleton<ISearchBinder<UserDto>, UserDtoSearchBinder>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

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
}

app.UseHttpsRedirection();

app.MapGet("/users", (ApplicationDbContext db, [FromServices] IUserService userService, [AsParameters] Query query) =>
{
    var (users, count) = userService.GetAllUsers().Apply(query);

    return TypedResults.Ok(new PagedResponse<object>(users.ToDynamicList(), count));
});

app.Run();
