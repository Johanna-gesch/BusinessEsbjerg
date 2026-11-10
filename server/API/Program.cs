
using API;
using Infra;
using Infra.Repositories;
using LinqToDB;
using Service;
using Service.Security;

var builder = WebApplication.CreateBuilder(args);

var options = new DataOptions().UsePostgreSQL(builder.Configuration["DB"] ?? "DataSource=dev.db");
var dataOptions = new DataOptions<MyDatabaseConnection>(options);

builder.Services.AddScoped<MyDatabaseConnection>(_ => new MyDatabaseConnection(dataOptions));

builder.Services.ConfigureDomainServices(
    builder.Configuration["JwtSecret"]
    ?? throw new InvalidOperationException("JwtSecret is missing"));


builder.Services.AddScoped<MySeeder>();

builder.Services.AddControllers();
builder.Services.AddCors();

var app = builder.Build();

app.UseCors(config => config.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin().SetIsOriginAllowed(_ => true));
app.UseAuthentication();
app.UseAuthorization();
app.UseOpenApi();
app.UseSwaggerUi();

using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<MySeeder>();
    seeder.Seed();
}

app.MapControllers();

app.Run();
