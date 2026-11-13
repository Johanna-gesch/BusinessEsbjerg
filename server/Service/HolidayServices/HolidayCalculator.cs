using Infra;

namespace Service.HolidayServices;

public class HolidayCalculator
{
    public static decimal CalculateHours(
        DateOnly start, DateOnly end,
        Func<DayOfWeek, decimal> hoursFor,
        IReadOnlySet<DateOnly> holidays)
    {
        if (end < start)
            throw new ValidationException("End date must be on or after start date");

        decimal total = 0;

        for (var day = start; day <= end; day = day.AddDays(1))
        {
            if (holidays.Contains(day)) continue; // holiday cost 0
            total += hoursFor(day.DayOfWeek); // weekends are 0 i workweek
        }

        return total;
    }
}