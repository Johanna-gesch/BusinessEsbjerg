
using Infra.Entities;
using Infra.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.IntegrationTests.TestClasses;

public class BackgroundServiceIntegrationTests
{
    
    
    [Fact]
    public async Task BackgroundService_OnStartup_ProcessMonthlyEarnings()
    {
        string? createdUserId = null;

        // Setup factory and helpers outside of the try block
        // First factory: BackgroundService is disabled, so we can create our testdata
        using var setupFactory = new CustomWebApplicationFactory();
        using var setupClient = setupFactory.CreateClient();

        var helpers = new HelperMethods(setupFactory, setupClient);
        
        try
        {
            // Arrange
            var (userId, accounts) = await helpers.CreateUserWithAccounts();

            createdUserId = userId;

            // Use previous completed month
            var today = DateOnly.FromDateTime(DateTime.Today);
            var monthStart = new DateOnly(today.Year, today.Month, 1).AddMonths(-1);

            await helpers.CreateWorkScheduleHistory(
                userId,
                monthStart,
                7.4m, 7.4m, 7.4m, 7.4m, 7.4m
            );

            var week1_5Account = accounts.Single(a => a.Type == HolidayType.Week1_5);
            var week6Account = accounts.Single(a => a.Type == HolidayType.Week6);

            // Second factory: BackgroundService is enabled.
            using var backgroundFactory = new BackgroundServiceWebApplicationFactory();

            using var backgroundClient = backgroundFactory.CreateClient();

            // Act

            // Starting the host starts the BackgroundService.
            // It should now process the previous completed month.
            var timeout = DateTime.UtcNow.AddSeconds(30);
            bool earningsCreated = false;

            while (DateTime.UtcNow < timeout)
            {
                using var scope = backgroundFactory.Services.CreateScope();

                var earningRepo = scope.ServiceProvider
                    .GetRequiredService<IEarningRepo>();

                // Only check earnings for the test user
                var week1_5Earnings = await earningRepo.GetEarningsByUserAndAccount(
                    userId, week1_5Account.Id);

                var week6Earnings = await earningRepo.GetEarningsByUserAndAccount(
                    userId, week6Account.Id);

                if (week1_5Earnings.Count == 1 && week6Earnings.Count == 1)
                {
                    earningsCreated = true;
                    break;
                }

                await Task.Delay(500);
            }

            Assert.True(earningsCreated,
                "BackgroundService did not create earnings for the test user within 30 seconds.");

            // Assert

            using var assertScope = backgroundFactory.Services.CreateScope();

            var assertRepo = assertScope.ServiceProvider.GetRequiredService<IEarningRepo>();

            var finalWeek1_5Earnings = await assertRepo.GetEarningsByUserAndAccount(
                userId, week1_5Account.Id);

            var finalWeek6Earnings = await assertRepo.GetEarningsByUserAndAccount(
                userId, week6Account.Id);

            Assert.Single(finalWeek1_5Earnings);
            Assert.Single(finalWeek6Earnings);

            Assert.True(finalWeek1_5Earnings.Single().EarnedDays > 0);
            Assert.True(finalWeek6Earnings.Single().EarnedHours > 0);
        }
        finally
        {
            //Cleanup
            if (createdUserId != null) 
                await helpers.RemoveUserWithAccountsAndSchedulesAndEarnings(createdUserId);
        }
        
        
    }
}
