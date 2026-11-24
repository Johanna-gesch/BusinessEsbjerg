using Infra;
using Infra.Entities.HolidayTracking.Requests;
using Infra.Entities.HolidayTracking.WorkSchedule;
using Infra.Repositories.Interfaces;
using Service.DTOs.SharedCalendarDTos;

namespace Service.HolidayServices;

/*
 * Builds the shared calendar. For every employee with a work schedule it tells,
 * for each day in a period, it tells the status of the employee.
 */

public class SharedCalendarService(
    ISharedCalendarRepo repo,
    IUserRepo userRepo,
    IWorkScheduleRepo workScheduleRepo, 
    CalendarDayService calendarDayService)
{
    private const int MaxPeriodInDays = 366;
    
    // For every employee, tells for each day in the period whether the
    // employee is at work, on holiday or off.

    public async Task<List<SharedCalendarEmployeeDto>> GetCalendar(DateOnly start, DateOnly end)
    {
        if (end < start)
            throw new ValidationException("End date must be on or after start date");
        if (end.DayNumber - start.DayNumber > MaxPeriodInDays)
            throw new ValidationException($"The period cannot be longer than {MaxPeriodInDays} days");
        
        // Four queries in total, and it doesn't matter how many employees there are
        var users = await userRepo.GetAll();
        var schedules = await workScheduleRepo.GetAllHistoryUpTo(end);
        var publicHolidays = await calendarDayService.GetHolidayDates(start, end);
        
        // Only approved holidays are shown to employees
        var approvedRequests = await repo.GetOverlapping(start, end, HolidayRequestStatus.Approved);

        var result = new List<SharedCalendarEmployeeDto>();

        foreach (var user in users.OrderBy(u => u.Username))
        {
            // This user's work schedules, newest workweek first
            var usersSchedules = schedules
                .Where(s => s.UserId == user.UserId)
                .OrderByDescending(s => s.EffectiveFrom)
                .ToList();
            
            // Without a work schedule we can't tell when the user works.
            if(usersSchedules.Count == 0)
                continue;

            var userRequests = approvedRequests.Where(r => r.UserId == user.UserId)
                .ToList();

            var days = new List<SharedCalendarDayDto>();
            for (var date = start; date <= end; date = date.AddDays(1))
            {
                days.Add(new SharedCalendarDayDto
                {
                    Date = date,
                    Status = GetStatus(date, usersSchedules, publicHolidays, userRequests)
                });
            }
            result.Add(new SharedCalendarEmployeeDto
            {
                UserId = user.UserId,
                Username = user.Username,
                Days = days,
            });
        }

        return result;
    }

    
    /*
     * Decides the status of one day for one employee. The order of the checks matters:
     * Public holiday first, then days the employee does not 
     */
    private static SharedCalendarDayStatus GetStatus(
        DateOnly date,
        List<WorkScheduleHistory> schedulesNewestFirst,
        HashSet<DateOnly> publicHolidays,
        List<HolidayRequest> approvedRequests)
    {
        // Nobody works on a public holiday
        if (publicHolidays.Contains(date))
            return SharedCalendarDayStatus.PublicHoliday;
        
        // The work schedule that applied on this date is the newest one that had started.
        var schedule = schedulesNewestFirst.FirstOrDefault(s => s.EffectiveFrom <= date);
        if (schedule is null || HoursFor(schedule, date.DayOfWeek) == 0)
            return SharedCalendarDayStatus.Off;
        
        // A normal work day: on holiday if an approved request covers it.
        var onHoliday = approvedRequests.Any(r => r.StartDate <= date && date <= r.EndDate);
        return onHoliday ? SharedCalendarDayStatus.Holiday : SharedCalendarDayStatus.Working;
    }

    private static decimal HoursFor(WorkScheduleHistory schedule, DayOfWeek day) => day switch
    {
        DayOfWeek.Monday => schedule.MondayHours,
        DayOfWeek.Tuesday => schedule.TuesdayHours,
        DayOfWeek.Wednesday => schedule.WednesdayHours,
        DayOfWeek.Thursday => schedule.ThursdayHours,
        DayOfWeek.Friday => schedule.FridayHours,
        _ => 0m
    };
}