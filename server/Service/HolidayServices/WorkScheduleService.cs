using Infra;
using Infra.Repositories.Interfaces;
using Service.DTOs.WorkScheduleDTOs;

namespace Service.HolidayServices;

public class WorkScheduleService (IWorkScheduleRepo repo)
{
    /*
     * This method is for retrieving the work schedule for individual dates.
     */
    public async Task<WorkScheduleDto> GetEffective(string userId, DateOnly date)
    {
        var current = await repo.GetCurrent(userId);
        if (current != null && current.EffectiveFrom <= date)
        {
            return new WorkScheduleDto
            {
                EffectiveFrom = current.EffectiveFrom,
                Monday = current.MondayHours,
                Tuesday = current.TuesdayHours,
                Wednesday = current.WednesdayHours,
                Thursday = current.ThursdayHours,
                Friday = current.FridayHours,
            };
        }

        var history = await repo.GetHistoryEffective(userId, date)
                      ?? throw new NotFoundException($"No work schedule for user {userId} on {date}");

        return new WorkScheduleDto
        {
            EffectiveFrom = history.EffectiveFrom,
            Monday = history.MondayHours,
            Tuesday = history.TuesdayHours,
            Wednesday = history.WednesdayHours,
            Thursday = history.ThursdayHours,
            Friday = history.FridayHours,
        };
    }

    /*
     * This method is designed to get schedules relevant to a period of time.
     */
    public async Task<List<WorkScheduleDto>> GetSchedulesForPeriod(
        string userId,
        DateOnly start,
        DateOnly end)
    {
        if (start > end)
            throw new ValidationException("Start date can't be after the end date.");

        var schedules = await repo.GetSchedulesForPeriod(userId, start, end);

        return schedules.Select(s => new WorkScheduleDto
        {
            EffectiveFrom = s.EffectiveFrom,
            Monday = s.MondayHours,
            Tuesday = s.TuesdayHours,
            Wednesday = s.WednesdayHours,
            Thursday = s.ThursdayHours,
            Friday = s.FridayHours,
        }).ToList();
    }

    /*
     * This method retrieves the schedules for a period of time.
     * It then uses HolidayCalculator to calculate the required hours.
     */
    public async Task<decimal> CalculateRequiredHours(
        string userId,
        DateOnly start,
        DateOnly end,
        IReadOnlySet<DateOnly> publicHolidays)
    {
        var schedules = await GetSchedulesForPeriod(userId, start, end);

        return HolidayCalculator.CalculateHours(start, end, date =>
            {
                var schedule = schedules
                                   .Where(s => s.EffectiveFrom <= date)
                                   .OrderByDescending(s => s.EffectiveFrom)
                                   .FirstOrDefault()
                               ?? throw new NotFoundException($"No work schedule exists for {date}.");
                
                return date.DayOfWeek switch
                {
                    DayOfWeek.Monday => schedule.Monday,
                    DayOfWeek.Tuesday => schedule.Tuesday,
                    DayOfWeek.Wednesday => schedule.Wednesday,
                    DayOfWeek.Thursday => schedule.Thursday,
                    DayOfWeek.Friday => schedule.Friday,
                    _ => 0
                };
            },
            publicHolidays);
    }
    
    /*
     * Retrieves the effective work schedule for a date
     * and calculates the employee's total weekly hours.
     */
    public async Task<decimal> GetWeeklyHours(string userId, DateOnly date)
    {
        var schedule = await GetEffective(userId, date);

        return schedule.Monday
               + schedule.Tuesday
               + schedule.Wednesday
               + schedule.Thursday
               + schedule.Friday;
    }
}