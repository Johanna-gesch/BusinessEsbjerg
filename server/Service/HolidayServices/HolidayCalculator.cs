using Infra;

namespace Service.HolidayServices;


/**
 * This calculator is used to calculate hours. 
 */
public class HolidayCalculator
{
    public static decimal CalculateHours(
        DateOnly start, DateOnly end,
        Func<DateOnly, decimal> hoursFor,
        IReadOnlySet<DateOnly> holidays)
    {
        if (end < start)
            throw new ValidationException("End date must be on or after start date");

        decimal total = 0;

        for (var date = start; date <= end; date = date.AddDays(1))
        {
            if (holidays.Contains(date)) continue; // holiday cost 0
            total += hoursFor(date); // weekends are 0 i workweek
        }

        return total;
    }
}