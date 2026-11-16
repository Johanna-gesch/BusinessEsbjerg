using Infra.Entities.HolidayTracking;
using Infra.Repositories.Interfaces;
using LinqToDB;
using LinqToDB.Async;

namespace Infra.Repositories.HolidayRepos;

public class CalendarDayRepo(MyDatabaseConnection dc) : ICalendarDayRepo
{
    public async Task Insert(CalendarDay calendarDay)
    {
        await dc.InsertAsync(calendarDay);
    }

    public async Task<CalendarDay?> GetByDate(DateOnly date)
    {
        return await dc.GetTable<CalendarDay>()
            .FirstOrDefaultAsync(day => day.Date == date);
    }

    public async Task<List<CalendarDay>> GetBetween(
        DateOnly startDate, DateOnly endDate)
    {
        return await dc.GetTable<CalendarDay>()
            .Where(day => day.Date >= startDate && day.Date <= endDate)
            .OrderBy(day => day.Date)
            .ToListAsync();

    }
}