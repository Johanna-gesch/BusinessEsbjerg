
using API;
using Infra;
using Infra.Repositories;
using Infra.Repositories.Interfaces;
using LinqToDB;
using Service;

var builder = WebApplication.CreateBuilder(args);

var options = new DataOptions().UsePostgreSQL(builder.Configuration["DB"] ?? "DataSource=dev.db");
var dataOptions = new DataOptions<MyDatabaseConnection>(options);

builder.Services.AddScoped<MyDatabaseConnection>(_ => new MyDatabaseConnection(dataOptions));

builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<IUserRepo, UserRepo>();

builder.Services.AddScoped<MySeeder>();

builder.Services.AddOpenApiDocument(settings => settings.SchemaSettings.SchemaProcessors.Add(new RequireNotNullableSchemaProcessor()));
builder.Services.AddControllers();
builder.Services.AddCors();

var app = builder.Build();

app.UseCors(config => config.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin().SetIsOriginAllowed(_ => true));
app.UseOpenApi();
app.UseSwaggerUi();

using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<MySeeder>();
    seeder.Seed();
}

app.MapControllers();

app.Run();
