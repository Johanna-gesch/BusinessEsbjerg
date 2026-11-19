using Infra.Entities.HolidayTracking.WorkSchedule;
using Infra.Repositories.Interfaces;

namespace Infra.Repositories.HolidayRepos;

public class SharedCalendarRepo (MyDatabaseConnection dc) : ISharedCalendarRepo
{
    public Task<List<WorkScheduleHistory>> GetAllHistoryUpTo(DateOnly end)
    {
        throw new NotImplementedException();
    }
}