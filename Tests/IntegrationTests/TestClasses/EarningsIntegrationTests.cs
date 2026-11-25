using Infra;
using Infra.Entities;
using Infra.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Service.HolidayServices.Earnings;

namespace Tests.IntegrationTests.TestClasses;

[Collection("Integration tests")]
public class EarningsIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HelperMethods _helpers;

    public EarningsIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _helpers = new HelperMethods(factory, factory.CreateClient());
    }

    [Fact]
    public async Task ProcessMonthlyEarningsForUser_WithValidData_CreatesEarnings()
    {
        string? createdUserId = null;

        try
        {
            // Arrange
            var (userId, accounts) = await _helpers.CreateUserWithAccounts();

            createdUserId = userId;

            var week1_5Account = accounts.Single(a => a.Type == HolidayType.Week1_5);
            var week6Account = accounts.Single(a => a.Type == HolidayType.Week6);

            var monthStart = new DateOnly(2026, 9, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            using var scope = _factory.Services.CreateScope();

            var earningService = scope.ServiceProvider.GetRequiredService<EarningService>();
            var earningRepo = scope.ServiceProvider.GetRequiredService<IEarningRepo>();

            await _helpers.CreateWorkScheduleHistory(
                userId,
                monthStart,
                7.4m,
                7.4m,
                7.4m,
                7.4m,
                7.4m
            );

            // Act
            await earningService.ProcessMonthlyEarningsForUser(
                userId,
                monthStart,
                monthEnd
            );

            // Assert
            var week1_5Earnings = await earningRepo.GetEarningsByUserAndAccount(
                userId,
                week1_5Account.Id
            );

            var week6Earnings = await earningRepo.GetEarningsByUserAndAccount(
                userId,
                week6Account.Id
            );

            var week1_5 = Assert.Single(week1_5Earnings);
            var week6 = Assert.Single(week6Earnings);

            // Week1_5
            Assert.Equal(userId, week1_5.UserId);
            Assert.Equal(week1_5Account.Id, week1_5.HolidayAccountId);
            Assert.Equal(monthStart, week1_5.EarnedDate);
            Assert.Equal(new DateOnly(2028, 12, 31), week1_5.ExpiryDate);
            Assert.Equal(2.08m, week1_5.EarnedDays);
            Assert.Equal(15.392m, week1_5.EarnedHours);

            // Week6
            Assert.Equal(userId, week6.UserId);
            Assert.Equal(week6Account.Id, week6.HolidayAccountId);
            Assert.Equal(monthStart, week6.EarnedDate);
            Assert.Equal(new DateOnly(2028, 4, 30), week6.ExpiryDate);
            Assert.Equal(3.083m, week6.EarnedHours);
            Assert.Equal(3.083m / 7.4m, week6.EarnedDays);
        }
        finally
        {
            //Cleanup
            if (createdUserId != null) 
                await _helpers.RemoveUserWithAccountsAndSchedulesAndEarnings(createdUserId);
        }
        
        
    }
    
}