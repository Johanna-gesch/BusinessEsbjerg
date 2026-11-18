
using Infra.Entities.HolidayTracking.WorkSchedule;

namespace Service.HolidayServices.Earnings;

public class EarningCalculator
{
    public record MonthlyEarningResult(
        decimal Week1_5Days,
        decimal Week1_5Hours,
        decimal Week6Hours,
        decimal Week6Days
    );

    private record SchedulePeriod(
        WorkScheduleHistory Schedule,
        DateOnly StartDate,
        DateOnly EndDate
    );

    public MonthlyEarningResult CalculateMonthlyEarnings(
        List<WorkScheduleHistory> schedules,
        DateOnly monthStart,
        DateOnly monthEnd)
    {
        if (monthStart > monthEnd)
        {
            throw new ArgumentException("Month start cannot be after month end.");
        }

        var periods = GetSchedulePeriods(schedules, monthStart, monthEnd);

        var totalWorkDaysPrMonth = periods.Sum(p =>
            CountMonthlyWorkDays(p.Schedule, p.StartDate, p.EndDate));

        if (totalWorkDaysPrMonth == 0)
        {
            return new MonthlyEarningResult(0, 0, 0, 0);
        }

        decimal week1_5Days = 0;
        decimal week1_5Hours = 0;
        decimal week6Hours = 0;
        decimal week6Days = 0;

        foreach (var period in periods)
        {
            int workDaysPrPeriod = CountMonthlyWorkDays(
                period.Schedule,
                period.StartDate,
                period.EndDate
            );

            if (workDaysPrPeriod == 0)
                continue;

            decimal shareOfMonth = (decimal)workDaysPrPeriod / totalWorkDaysPrMonth;

            decimal weeklyHours = CalculateWeeklyHours(period.Schedule);
            decimal averageDailyHours = weeklyHours / CountWeeklyWorkDays(period.Schedule);

            decimal earnedWeek1_5Days =
                2.08m * (weeklyHours / 37m) * shareOfMonth;

            decimal earnedWeek1_5Hours =
                earnedWeek1_5Days * averageDailyHours;
            
            decimal earnedWeek6Hours =
                3.083m * (weeklyHours / 37m) * shareOfMonth;

            decimal earnedWeek6Days =
                earnedWeek6Hours / averageDailyHours;

            week1_5Days += earnedWeek1_5Days;
            week1_5Hours += earnedWeek1_5Hours;
            week6Hours += earnedWeek6Hours;
            week6Days += earnedWeek6Days;
        }

        return new MonthlyEarningResult(
            week1_5Days,
            week1_5Hours,
            week6Hours,
            week6Days
        );
    }

    private List<SchedulePeriod> GetSchedulePeriods(
        List<WorkScheduleHistory> schedules,
        DateOnly monthStart,
        DateOnly monthEnd)
    {
        var orderedSchedules = schedules
            .Where(s => s.EffectiveFrom <= monthEnd)
            .OrderBy(s => s.EffectiveFrom)
            .ToList();

        var periods = new List<SchedulePeriod>();

        for (int i = 0; i < orderedSchedules.Count; i++)
        {
            var schedule = orderedSchedules[i];

            var startDate = schedule.EffectiveFrom < monthStart
                ? monthStart
                : schedule.EffectiveFrom;

            var endDate = i + 1 < orderedSchedules.Count
                ? orderedSchedules[i + 1].EffectiveFrom.AddDays(-1)
                : monthEnd;

            if (endDate > monthEnd)
                endDate = monthEnd;

            if (startDate <= endDate)
            {
                periods.Add(new SchedulePeriod(
                    schedule,
                    startDate,
                    endDate
                ));
            }
        }

        return periods;
    }

    private int CountMonthlyWorkDays(
        WorkScheduleHistory schedule,
        DateOnly startDate,
        DateOnly endDate)
    {
        int workDays = 0;

        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            decimal hours = date.DayOfWeek switch
            {
                DayOfWeek.Monday => schedule.MondayHours,
                DayOfWeek.Tuesday => schedule.TuesdayHours,
                DayOfWeek.Wednesday => schedule.WednesdayHours,
                DayOfWeek.Thursday => schedule.ThursdayHours,
                DayOfWeek.Friday => schedule.FridayHours,
                _ => 0
            };

            if (hours > 0)
            {
                workDays++;
            }
        }

        return workDays;
    }

    private decimal CalculateWeeklyHours(WorkScheduleHistory schedule)
    {
        return schedule.MondayHours +
               schedule.TuesdayHours +
               schedule.WednesdayHours +
               schedule.ThursdayHours +
               schedule.FridayHours;
    }

    private int CountWeeklyWorkDays(WorkScheduleHistory schedule)
    {
        int workDays = 0;

        if (schedule.MondayHours > 0) workDays++;
        if (schedule.TuesdayHours > 0) workDays++;
        if (schedule.WednesdayHours > 0) workDays++;
        if (schedule.ThursdayHours > 0) workDays++;
        if (schedule.FridayHours > 0) workDays++;

        return workDays;
    }
}
