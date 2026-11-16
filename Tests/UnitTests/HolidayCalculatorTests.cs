using Infra;
using Service.HolidayServices;

namespace Tests.UnitTests;

public class HolidayCalculatorTests
{
    // Man-Tor 7,5, Fre 7, weekends 0
    private static decimal Hours(DayOfWeek d) => d switch
    {
        DayOfWeek.Friday => 7m,
        DayOfWeek.Saturday or DayOfWeek.Sunday => 0m,
        _ => 7.5m
    };
    
    private static readonly HashSet<DateOnly> NoHolidays = new();

    [Fact]
    public void FullWeekWithoutHolidays_Costs37Hours()
    {
        // 12. okt 2026 is a monday
        var result = HolidayCalculator.CalculateHours(
            new DateOnly(2026, 10, 12), new DateOnly(2026, 10, 16), Hours, NoHolidays);

        Assert.Equal(37m, result);
    }
    
    [Fact]
    public void HolidayOnSaturday_ChangesNothing()
    {
        var holidays = new HashSet<DateOnly> { new(2026, 10, 17) };   // Saturday

        var result = HolidayCalculator.CalculateHours(
            new DateOnly(2026, 10, 12), new DateOnly(2026, 10, 18), Hours, holidays);

        Assert.Equal(37m, result);
    }

    [Fact]
    public void OnlyAHoliday_CostsZero()
    {
        var holidays = new HashSet<DateOnly> { new(2026, 12, 24) };

        var result = HolidayCalculator.CalculateHours(
            new DateOnly(2026, 12, 24), new DateOnly(2026, 12, 24), Hours, holidays);

        Assert.Equal(0m, result);
    }

    [Fact]
    public void EndBeforeStart_ThrowsValidationException()
    {
        Assert.Throws<ValidationException>(() => HolidayCalculator.CalculateHours(
            new DateOnly(2026, 10, 16), new DateOnly(2026, 10, 12), Hours, NoHolidays));
    }
    
    [Fact]
    public void HolidayOnThursday_IsNotCharged()
    {
        var holidays = new HashSet<DateOnly> { new(2026, 10, 15) };   // Thursday

        var result = HolidayCalculator.CalculateHours(
            new DateOnly(2026, 10, 12), new DateOnly(2026, 10, 16), Hours, holidays);

        Assert.Equal(29.5m, result);
    }
    
    
}