using API.BackgroundTasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Tests.IntegrationTests;

public class CustomWebApplicationFactory : BaseWebApplicationFactory
{
    protected override void ConfigureAdditionalServices(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Find the registered EarningsBackgroundService.
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IHostedService) &&
                     d.ImplementationType == typeof(EarningsBackgroundService)
            );

            // Remove the background service so it does not run during the tests.
            if (descriptor != null)
                services.Remove(descriptor);
        });
    }
}