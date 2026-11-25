using API;
using Microsoft.Extensions.DependencyInjection;
using Tests.IntegrationTests;

namespace Tests.TestClasses;

public class TestDatabaseFixture : IAsyncLifetime
{
    private CustomWebApplicationFactory _factory = null!;
    private HttpClient _client = null!;

    public Task InitializeAsync()
    {
        // Start testapplikationen.
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();

        // Kør test-seederen én gang.
        using var scope = _factory.Services.CreateScope();

        var seeder = scope.ServiceProvider.GetRequiredService<MySeeder>();
        seeder.SeedForTests();

        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        _client.Dispose();
        _factory.Dispose();

        return Task.CompletedTask;
    }
}