using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace Tests.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DB"] = Environment.GetEnvironmentVariable("TEST_DB")
                         ?? throw new InvalidOperationException("TEST_DB is not set."),
                
                ["JwtSecret"] = Environment.GetEnvironmentVariable("JwtSecret")
                                ?? throw new InvalidOperationException("JwtSecret is not set.")

            });
            Console.WriteLine("CUSTOM TEST CONFIGURATION LOADED");
        });
    }
    
}