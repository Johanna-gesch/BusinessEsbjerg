using API;
using Infra;
using Infra.Repositories;
using Infra.Repositories.Interfaces;
using LinqToDB;
using LinqToDB.AspNet;
using Service;

var builder = WebApplication.CreateBuilder(args);

// Register the database context with dependency injection.
builder.Services.AddLinqToDBContext<MyDatabaseConnection>((provider, options) =>
        options.UsePostgreSQL(
            builder.Configuration["DB"]
            ?? throw new InvalidOperationException("Missing DB configuration")
        )
);

builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<IUserRepo, UserRepo>();

builder.Services.AddScoped<MySeeder>();
builder.Services.AddExceptionHandler<ProblemExceptionHandler>();

builder.Services.AddOpenApiDocument(settings => settings.SchemaSettings.SchemaProcessors.Add(new RequireNotNullableSchemaProcessor()));
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
app.UseOpenApi();
app.UseSwaggerUi();
app.UseExceptionHandler();


if (app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();

    var seeder = scope.ServiceProvider.GetRequiredService<MySeeder>();
    seeder.InitializeDatabase();
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