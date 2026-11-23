using Infra.Entities.HolidayTracking.Requests;
using Infra.Entities.HolidayTracking.WorkSchedule;
using Infra.Repositories.Interfaces;
using LinqToDB;
using LinqToDB.Async;

namespace Infra.Repositories.HolidayRepos;

public class SharedCalendarRepo (MyDatabaseConnection dc) : ISharedCalendarRepo
{
    public async Task<List<HolidayRequest>> GetOverlapping(DateOnly start, DateOnly end, HolidayRequestStatus status)
    {
        return await dc.HolidayRequests
            .Where(r => r.Status == status && r.StartDate <= end && r.EndDate >= start)
            .OrderBy(r => r.StartDate)
            .ToListAsync();
    }

    
}