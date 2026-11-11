using API;
using Infra;
using LinqToDB;
using LinqToDB.AspNet;

var builder = WebApplication.CreateBuilder(args);

// Register the database context with dependency injection.
builder.Services.AddLinqToDBContext<MyDatabaseConnection>((provider, options) =>
        options.UsePostgreSQL(
            builder.Configuration["DB"]
            ?? throw new InvalidOperationException("Missing DB configuration")
        )
);

builder.Services.ConfigureDomainServices(
    builder.Configuration["JwtSecret"]
    ?? throw new InvalidOperationException("JwtSecret is missing"));


builder.Services.AddScoped<MySeeder>();
builder.Services.AddExceptionHandler<ProblemExceptionHandler>();

builder.Services.AddControllers();

builder.Services.AddProblemDetails();

var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors(config => config.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin().SetIsOriginAllowed(_ => true));
app.UseAuthentication();
app.UseAuthorization();
app.UseOpenApi();
app.UseSwaggerUi();
app.UseExceptionHandler();


if (app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();

    var seeder = scope.ServiceProvider.GetRequiredService<MySeeder>();
    seeder.SeedForTests();
}
else
{
    using var scope = app.Services.CreateScope();

    var seeder = scope.ServiceProvider.GetRequiredService<MySeeder>();
    seeder.Seed();
}

app.MapControllers();

app.Run();


public partial class Program { }