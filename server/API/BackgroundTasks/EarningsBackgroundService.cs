using Service.HolidayServices.Earnings;

namespace API.BackgroundTasks;

public class EarningsBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<EarningsBackgroundService> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();

                var earningService = scope.ServiceProvider
                    .GetRequiredService<EarningService>();

                await earningService.ProcessMonthlyEarnings(
                    stoppingToken
                );

                logger.LogInformation(
                    "Monthly earnings processing completed."
                );
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "An error occurred while processing monthly earnings."
                );
            }

            try
            {
                await Task.Delay(
                    TimeSpan.FromDays(1),
                    stoppingToken
                );
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }
}