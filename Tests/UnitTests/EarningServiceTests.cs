
using Infra;
using Infra.Entities;
using Infra.Entities.HolidayTracking.Accounts;
using Infra.Entities.HolidayTracking.Earnings;
using Infra.Entities.HolidayTracking.WorkSchedule;
using Service.HolidayServices.Earnings;
using Tests.UnitTests.Stubs;

namespace Tests.UnitTests;

public class EarningServiceTests
{
    private readonly EarningRepoStub _earningRepoStub = new();
    private readonly AccountRepoStub _accountRepoStub = new();
    private readonly WorkScheduleRepoStub _workScheduleRepoStub = new();

    private readonly EarningCalculator _earningCalculator = new();

    private const string UserId = "employee@test.com";

    private readonly Guid _week1_5AccountId = Guid.NewGuid();
    private readonly Guid _week6AccountId = Guid.NewGuid();

    private EarningService CreateService()
    {
        return new EarningService(
            _earningCalculator,
            _earningRepoStub,
            _accountRepoStub,
            null!, // IUserRepo is not used by CalculateProjectedEarnings
            _workScheduleRepoStub
        );
    }

    private void AddAccounts()
    {
        _accountRepoStub.Accounts.Add(new HolidayAccount
        {
            Id = _week1_5AccountId,
            UserId = UserId,
            Type = HolidayType.Week1_5
        });

        _accountRepoStub.Accounts.Add(new HolidayAccount
        {
            Id = _week6AccountId,
            UserId = UserId,
            Type = HolidayType.Week6
        });
    }

    private void AddSchedule(
        DateOnly effectiveFrom,
        decimal monday = 7.4m,
        decimal tuesday = 7.4m,
        decimal wednesday = 7.4m,
        decimal thursday = 7.4m,
        decimal friday = 7.4m)
    {
        _workScheduleRepoStub.History.Add(new WorkScheduleHistory
        {
            Id = Guid.NewGuid(),
            UserId = UserId,
            EffectiveFrom = effectiveFrom,
            MondayHours = monday,
            TuesdayHours = tuesday,
            WednesdayHours = wednesday,
            ThursdayHours = thursday,
            FridayHours = friday
        });
    }

    private decimal CalculateExpectedHours(
        DateOnly fromMonth,
        DateOnly checkpoint,
        HolidayType type)
    {
        var schedules = _workScheduleRepoStub.History;

        decimal total = 0;

        for (var month = fromMonth;
             month <= checkpoint;
             month = month.AddMonths(1))
        {
            var monthEnd = month.AddMonths(1).AddDays(-1);

            var result = _earningCalculator.CalculateMonthlyEarnings(
                schedules,
                month,
                monthEnd
            );

            total += type == HolidayType.Week1_5
                ? result.Week1_5Hours
                : result.Week6Hours;
        }

        return decimal.Round(total, 2);
    }

    #region CalculateProjectedEarnings

    [Fact]
    public async Task CalculateProjectedEarnings_ReturnsFutureEarnings()
    {
        // Arrange
        AddAccounts();

        var today = DateOnly.FromDateTime(DateTime.Today);
        var currentMonth = new DateOnly(today.Year, today.Month, 1);

        var checkpoint = currentMonth
            .AddMonths(2)
            .AddDays(-1);

        AddSchedule(currentMonth);

        var expected = CalculateExpectedHours(
            currentMonth,
            checkpoint,
            HolidayType.Week1_5
        );

        var service = CreateService();

        // Act
        var result = await service.CalculateProjectedEarnings(
            UserId,
            _week1_5AccountId,
            checkpoint
        );

        // Assert
        Assert.Equal(expected, result);
        Assert.True(result > 0);
    }

    [Fact]
    public async Task CalculateProjectedEarnings_DoesNotProjectAlreadyRecordedMonth()
    {
        // Arrange
        AddAccounts();

        var today = DateOnly.FromDateTime(DateTime.Today);
        var currentMonth = new DateOnly(today.Year, today.Month, 1);

        var nextMonth = currentMonth.AddMonths(1);
        var checkpoint = nextMonth.AddMonths(1).AddDays(-1);

        AddSchedule(currentMonth);

        // An earning already exists for the current month.
        _earningRepoStub.AddEarning(new HolidayEarning
        {
            Id = Guid.NewGuid(),
            UserId = UserId,
            HolidayAccountId = _week1_5AccountId,
            EarnedDate = currentMonth,
            EarnedHours = 15.39m,
            EarnedDays = 2.08m,
            ExpiryDate = new DateOnly(currentMonth.Year + 2, 12, 31)
        });

        var expected = CalculateExpectedHours(
            nextMonth,
            checkpoint,
            HolidayType.Week1_5
        );

        var service = CreateService();

        // Act
        var result = await service.CalculateProjectedEarnings(
            UserId,
            _week1_5AccountId,
            checkpoint
        );

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task CalculateProjectedEarnings_UsesScheduleChanges()
    {
        // Arrange
        AddAccounts();

        var today = DateOnly.FromDateTime(DateTime.Today);
        var currentMonth = new DateOnly(today.Year, today.Month, 1);

        var nextMonth = currentMonth.AddMonths(1);
        var checkpoint = nextMonth.AddMonths(1).AddDays(-1);

        // First schedule: 37 hours per week.
        AddSchedule(currentMonth);

        // Second schedule: 20 hours per week, effective next month.
        AddSchedule(
            nextMonth,
            monday: 4m,
            tuesday: 4m,
            wednesday: 4m,
            thursday: 4m,
            friday: 4m
        );

        var expected = CalculateExpectedHours(
            currentMonth,
            checkpoint,
            HolidayType.Week1_5
        );

        var service = CreateService();

        // Act
        var result = await service.CalculateProjectedEarnings(
            UserId,
            _week1_5AccountId,
            checkpoint
        );

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task CalculateProjectedEarnings_Week6_ReturnsZeroBeforeAvailabilityDate()
    {
        // Arrange
        AddAccounts();

        var today = DateOnly.FromDateTime(DateTime.Today);
        var currentMonth = new DateOnly(today.Year, today.Month, 1);

        // April 30 of next year is before the May 1 availability date
        // for earnings accrued during the current year.
        var checkpoint = new DateOnly(today.Year + 1, 4, 30);

        AddSchedule(currentMonth);

        var service = CreateService();

        // Act
        var result = await service.CalculateProjectedEarnings(
            UserId,
            _week6AccountId,
            checkpoint
        );

        // Assert
        Assert.Equal(0m, result);
    }

    [Fact]
    public async Task CalculateProjectedEarnings_Week6_IncludesEarningsAvailableByCheckpoint()
    {
        // Arrange
        AddAccounts();

        var today = DateOnly.FromDateTime(DateTime.Today);
        var currentMonth = new DateOnly(today.Year, today.Month, 1);

        var checkpoint = new DateOnly(today.Year + 1, 5, 31);

        AddSchedule(currentMonth);

        // Only earnings from the current year should be available
        // by May 31 of the following year.
        decimal expected = 0;

        for (var month = currentMonth;
             month.Year == today.Year;
             month = month.AddMonths(1))
        {
            var monthEnd = month.AddMonths(1).AddDays(-1);

            var result = _earningCalculator.CalculateMonthlyEarnings(
                _workScheduleRepoStub.History,
                month,
                monthEnd
            );

            expected += result.Week6Hours;
        }

        expected = decimal.Round(expected, 2);

        var service = CreateService();

        // Act
        var resultHours = await service.CalculateProjectedEarnings(
            UserId,
            _week6AccountId,
            checkpoint
        );

        // Assert
        Assert.Equal(expected, resultHours);
    }

    [Fact]
    public async Task CalculateProjectedEarnings_ThrowsWhenAccountDoesNotExist()
    {
        // Arrange
        AddAccounts();

        var service = CreateService();

        // Act
        var act = async () => await service.CalculateProjectedEarnings(
            UserId,
            Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.Today).AddYears(1)
        );

        // Assert
        await Assert.ThrowsAsync<ValidationException>(act);
    }

    [Fact]
    public async Task CalculateProjectedEarnings_ThrowsWhenNoScheduleExists()
    {
        // Arrange
        AddAccounts();

        var service = CreateService();

        // Act
        var act = async () => await service.CalculateProjectedEarnings(
            UserId,
            _week1_5AccountId,
            DateOnly.FromDateTime(DateTime.Today).AddYears(1)
        );

        // Assert
        await Assert.ThrowsAsync<ValidationException>(act);
    }

    #endregion
}
