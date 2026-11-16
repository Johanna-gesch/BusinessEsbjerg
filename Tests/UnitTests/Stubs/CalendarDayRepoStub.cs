using Infra.Entities.HolidayTracking;
using Infra.Repositories.Interfaces;

namespace Tests.UnitTests.Stubs;

public class CalendarDayRepoStub : ICalendarDayRepo
{
    public List<CalendarDay> Days { get; } = new();

    public Task Insert(CalendarDay calendarDay)
    {
        Days.Add(calendarDay);
        return Task.CompletedTask;
    }
    
    public Task<CalendarDay?> GetByDate(DateOnly date) =>
        Task.FromResult(Days.FirstOrDefault(d => d.Date == date));

    public Task<List<CalendarDay>> GetBetween(DateOnly start, DateOnly end) =>
        Task.FromResult(Days
            .Where(d => d.Date >= start && d.Date <= end)
            .OrderBy(d => d.Date)
            .ToList());
}