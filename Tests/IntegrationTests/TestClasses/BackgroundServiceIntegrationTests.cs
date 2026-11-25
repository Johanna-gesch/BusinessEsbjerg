
using Infra.Entities;
using Infra.Entities.HolidayTracking.Accounts;
using Infra.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.IntegrationTests.TestClasses;

[Collection("Integration tests")]
public class BackgroundServiceIntegrationTests
{ 

    [Fact]
    public async Task BackgroundService_OnStartup_ProcessMonthlyEarnings()
    {
        string? createdUserId = null;

        List<HolidayAccount> accounts = new();

        // ARRANGE
        // First factory: BackgroundService is disabled.
        using (var setupFactory = new CustomWebApplicationFactory())
        using (var setupClient = setupFactory.CreateClient())
        {
            var helpers = new HelperMethods(setupFactory, setupClient);

            var (userId, createdAccounts) = await helpers.CreateUserWithAccounts();

            createdUserId = userId;
            accounts = createdAccounts;

            // Use previous completed month.
            var today = DateOnly.FromDateTime(DateTime.Today);
            var monthStart = new DateOnly(today.Year, today.Month, 1).AddMonths(-1);

            await helpers.CreateWorkScheduleHistory(
                userId,
                monthStart,
                7.4m, 7.4m, 7.4m, 7.4m, 7.4m
            );
        } // setupFactory and setupClient are disposed here.

        try
        {
            // ACT
            // Second factory: BackgroundService is enabled.
            using var backgroundFactory = new BackgroundServiceWebApplicationFactory();

            using var backgroundClient = backgroundFactory.CreateClient();

            var week1_5Account = accounts.Single(a => a.Type == HolidayType.Week1_5);
            var week6Account = accounts.Single(a => a.Type == HolidayType.Week6);

            var timeout = DateTime.UtcNow.AddSeconds(30);
            bool earningsCreated = false;

            while (DateTime.UtcNow < timeout)
            {
                using var scope = backgroundFactory.Services.CreateScope();

                var earningRepo = scope.ServiceProvider
                    .GetRequiredService<IEarningRepo>();

                var week1_5Earnings = await earningRepo.GetEarningsByUserAndAccount(
                    createdUserId!, week1_5Account.Id);

                var week6Earnings = await earningRepo.GetEarningsByUserAndAccount(
                    createdUserId!, week6Account.Id);

                if (week1_5Earnings.Count == 1 && week6Earnings.Count == 1)
                {
                    earningsCreated = true;
                    break;
                }

                await Task.Delay(500);
            }

            Assert.True(earningsCreated,
                "BackgroundService did not create earnings for the test user within 30 seconds.");

            // ASSERT
            using var assertScope = backgroundFactory.Services.CreateScope();

            var assertRepo = assertScope.ServiceProvider
                .GetRequiredService<IEarningRepo>();

            var finalWeek1_5Earnings = await assertRepo.GetEarningsByUserAndAccount(
                createdUserId!, week1_5Account.Id);

            var finalWeek6Earnings = await assertRepo.GetEarningsByUserAndAccount(
                createdUserId!, week6Account.Id);

            Assert.Single(finalWeek1_5Earnings);
            Assert.Single(finalWeek6Earnings);

            Assert.True(finalWeek1_5Earnings.Single().EarnedDays > 0);
            Assert.True(finalWeek6Earnings.Single().EarnedHours > 0);
        }
        finally
        {
            // CLEANUP
            if (createdUserId != null)
            {
                // Use a fresh factory to access the database for cleanup.
                using var cleanupFactory = new CustomWebApplicationFactory();
                using var cleanupClient = cleanupFactory.CreateClient();

                var cleanupHelpers = new HelperMethods(cleanupFactory, cleanupClient);

                await cleanupHelpers.RemoveUserWithAccountsAndSchedulesAndEarnings(createdUserId);
            }
        }
    }
}
