using Infra.Entities.HolidayTracking;

namespace Infra.Repositories.Interfaces;

public interface ICalendarDayRepo
{
    Task Insert(CalendarDay calendarDay);
    Task<CalendarDay?> GetByDate(DateOnly date);
    Task<List<CalendarDay>> GetBetween(DateOnly startDate, DateOnly endDate);

}